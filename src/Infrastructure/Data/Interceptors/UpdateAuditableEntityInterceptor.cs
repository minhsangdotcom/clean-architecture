using Application.Common.Interfaces.Services.Accessors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SharedKernel.Entities;

namespace Infrastructure.Data.Interceptors;

public class UpdateAuditableEntityInterceptor(ICurrentUser currentUser) : SaveChangesInterceptor
{
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
        List<EntityEntry> entities =
        [
            .. context
                .ChangeTracker.Entries()
                .Where(e => e.Entity is AuditableEntity || e.Entity is AggregateRoot),
        ];

        foreach (EntityEntry entry in entities)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                {
                    if (currentUser.Id != Ulid.Empty)
                    {
                        entry.Property(nameof(IAuditable.CreatedBy)).CurrentValue =
                            currentUser.Id.ToString();
                    }
                    break;
                }
                case EntityState.Modified:
                {
                    if (currentUser.Id != Ulid.Empty)
                    {
                        entry.Property(nameof(IAuditable.UpdatedBy)).CurrentValue =
                            currentUser.Id.ToString();
                    }

                    entry.Property(nameof(IAuditable.UpdatedAt)).CurrentValue = currentTime;
                    break;
                }
                default:
                    break;
            }
        }
    }
}
