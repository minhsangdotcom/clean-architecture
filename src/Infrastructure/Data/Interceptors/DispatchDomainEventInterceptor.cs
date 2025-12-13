using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.DomainEvents;
using SharedKernel.Entities;

namespace Infrastructure.Data.Interceptors;

public class DispatchDomainEventInterceptor(IServiceScopeFactory serviceScopeFactory)
    : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        await DispatchDomainEvents(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public async Task DispatchDomainEvents(DbContext? context)
    {
        if (context == null)
            return;

        List<AggregateRoot> entities =
        [
            .. context
                .ChangeTracker.Entries<AggregateRoot>()
                .Where(e => e.Entity.UncommittedEvents.Count != 0)
                .Select(e => e.Entity),
        ];

        IEnumerable<IDomainEvent> domainEvents = entities.SelectMany(e => e.UncommittedEvents);
        entities.ForEach(e => e.DequeueUncommittedEvents());

        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IPublisher mediator = scope.ServiceProvider.GetRequiredService<IPublisher>();
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            await mediator.Publish(domainEvent);
        }
    }
}
