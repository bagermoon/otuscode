using Ardalis.SharedKernel;

using MongoDB.Driver;

namespace RestoRate.RatingService.Infrastructure.Data;

internal sealed class MongoContext(
    MongoUnitOfWork unitOfWork,
    IMongoCollectionProvider collections)
    : IMongoContext
{
    public IMongoCollection<T> Collection<T>() where T : class
        => collections.GetRequiredCollection<T>();

    public bool TryGet<TAggregate>(Guid id, out TAggregate aggregate)
        where TAggregate : EntityBase<Guid>
        => unitOfWork.TryGet(id, out aggregate);

    public void Attach<TAggregate>(TAggregate aggregate, EntityState state = EntityState.Unchanged)
        where TAggregate : EntityBase<Guid>
        => unitOfWork.Attach(aggregate, state);

    public void MarkNew<TAggregate>(TAggregate aggregate)
        where TAggregate : EntityBase<Guid>
        => unitOfWork.MarkNew(aggregate);

    public void MarkDirty<TAggregate>(TAggregate aggregate)
        where TAggregate : EntityBase<Guid>
        => unitOfWork.MarkDirty(aggregate);

    public void MarkDeleted<TAggregate>(TAggregate aggregate)
        where TAggregate : EntityBase<Guid>
        => unitOfWork.MarkDeleted(aggregate);
}
