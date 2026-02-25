namespace RestoRate.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
    Task FlushDomainEventsAsync(CancellationToken cancellationToken = default);
}
