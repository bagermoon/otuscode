using Ardalis.SharedKernel;

using MongoDB.Driver;

namespace RestoRate.RatingService.Infrastructure.Data;

public interface IRatingMongoContext : IUnitOfWork
{
    IMongoCollection<T> Collection<T>() where T : class;

    bool TryGet<TAggregate>(Guid id, out TAggregate aggregate)
        where TAggregate : EntityBase<Guid>;

    void Attach<TAggregate>(TAggregate aggregate, EntityState state = EntityState.Unchanged)
        where TAggregate : EntityBase<Guid>;

    void MarkNew<TAggregate>(TAggregate aggregate)
        where TAggregate : EntityBase<Guid>;

    void MarkDirty<TAggregate>(TAggregate aggregate)
        where TAggregate : EntityBase<Guid>;

    void MarkDeleted<TAggregate>(TAggregate aggregate)
        where TAggregate : EntityBase<Guid>;
}
