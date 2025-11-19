using Ardalis.SharedKernel;

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

using RestoRate.BuildingBlocks.Data.DbContexts;

namespace RestoRate.BuildingBlocks.Data.Interceptors;

public class EventDispatchInterceptor(IServiceScopeFactory scopeFactory) : SaveChangesInterceptor
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    // Called after SaveChangesAsync has completed successfully
    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
      CancellationToken cancellationToken = new CancellationToken())
    {
        var context = eventData.Context;
        if (context is not DbContextBase appDbContext)
        {
            return await base.SavedChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
        }

        // Retrieve all tracked entities that have domain events
        var entitiesWithEvents = appDbContext.GetEntitiesWithDomainEvents();

        if (!entitiesWithEvents.Any())
        {
            return await base.SavedChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
        }
        using var scope = _scopeFactory.CreateScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();

        // Dispatch and clear domain events
        await dispatcher.DispatchAndClearEvents(entitiesWithEvents);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);

    }
}
