using Ardalis.SharedKernel;

using MongoDB.Driver;

namespace RestoRate.RatingService.Infrastructure.Data;
/// <summary>
/// Простая реализация <see cref="IMongoAggregateWriter"/>.
/// Используется <see cref="MongoContext"/> для получения писателей для агрегатов.
/// Требуется для отделения логики получения коллекций от логики записи агрегатов в MongoDB.
/// </summary>
public sealed class MongoAggregateWriter<TAggregate>(IMongoCollection<TAggregate> collection) : IMongoAggregateWriter
    where TAggregate : EntityBase<Guid>
{
    public Type DocumentType => typeof(TAggregate);

    public Task InsertAsync(IClientSessionHandle? session, EntityBase<Guid> aggregate, CancellationToken cancellationToken)
    {
        var typed = (TAggregate)aggregate;
        return session is null
            ? collection.InsertOneAsync(typed, cancellationToken: cancellationToken)
            : collection.InsertOneAsync(session, typed, cancellationToken: cancellationToken);
    }

    public Task ReplaceAsync(IClientSessionHandle? session, EntityBase<Guid> aggregate, CancellationToken cancellationToken)
    {
        var typed = (TAggregate)aggregate;
        return session is null
            ? collection.ReplaceOneAsync(x => x.Id == typed.Id, typed, cancellationToken: cancellationToken)
            : collection.ReplaceOneAsync(session, x => x.Id == typed.Id, typed, cancellationToken: cancellationToken);
    }

    public Task DeleteAsync(IClientSessionHandle? session, Guid id, CancellationToken cancellationToken)
        => session is null
            ? collection.DeleteOneAsync(x => x.Id == id, cancellationToken: cancellationToken)
            : collection.DeleteOneAsync(session, x => x.Id == id, cancellationToken: cancellationToken);
}
