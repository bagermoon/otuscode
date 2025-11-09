using Ardalis.SharedKernel;

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

using RestoRate.BuildingBlocks.Data.DbContexts;

namespace RestoRate.BuildingBlocks.Data.Interceptors;

public class EventDispatchInterceptor : SaveChangesInterceptor
{
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
        var dispatcher = context.GetService<IDomainEventDispatcher>();

        // Dispatch and clear domain events
        await dispatcher.DispatchAndClearEvents(entitiesWithEvents);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);

    }
}
