using Application.Common.Interfaces.Services;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SharedKernel.Common;

namespace Infrastructure.Data.Interceptors;

public class UpdateAuditableEntityInterceptor(ICurrentUser currentUser) : SaveChangesInterceptor
{
    private const string ANONYMOUS_CREATED_BY = "SYSTEM";

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditableEntities(DbContext context)
    {
        DateTimeOffset currentTime = DateTimeOffset.UtcNow;
        var entities = context
            .ChangeTracker.Entries()
            .Where(e => e.Entity is AuditableEntity || e.Entity is AggregateRoot);

        foreach (EntityEntry entry in entities)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(nameof(IAuditable.CreatedBy)).CurrentValue =
                        currentUser.Id?.ToString() ?? ANONYMOUS_CREATED_BY;
                    break;
                case EntityState.Modified:
                    entry.Property(nameof(IAuditable.UpdatedBy)).CurrentValue =
                        currentUser.Id?.ToString() ?? ANONYMOUS_CREATED_BY;

                    entry.Property(nameof(IAuditable.UpdatedAt)).CurrentValue = currentTime;
                    break;
                default:
                    break;
            }
        }
    }
}
