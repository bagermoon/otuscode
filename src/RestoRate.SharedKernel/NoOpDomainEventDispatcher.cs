using Ardalis.SharedKernel;

namespace RestoRate.SharedKernel;

public sealed class NoOpDomainEventDispatcher : IDomainEventDispatcher
{
    public Task DispatchAndClearEvents(IEnumerable<IHasDomainEvents> entities)
    {
        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }
        return Task.CompletedTask;
    }
}
