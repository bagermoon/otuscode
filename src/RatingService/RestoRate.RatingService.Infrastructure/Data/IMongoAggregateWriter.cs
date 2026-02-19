using Ardalis.SharedKernel;

using MongoDB.Driver;

namespace RestoRate.RatingService.Infrastructure.Data;
public interface IMongoAggregateWriter
{
    Type DocumentType { get; }

    Task InsertAsync(IClientSessionHandle? session, EntityBase<Guid> aggregate, CancellationToken cancellationToken);
    Task ReplaceAsync(IClientSessionHandle? session, EntityBase<Guid> aggregate, CancellationToken cancellationToken);
    Task DeleteAsync(IClientSessionHandle? session, Guid id, CancellationToken cancellationToken);
}
