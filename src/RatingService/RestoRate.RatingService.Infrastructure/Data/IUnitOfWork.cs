using MongoDB.Driver;

namespace RestoRate.RatingService.Infrastructure.Data;

public interface IUnitOfWork
{
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);

    Task<bool> SaveEntitiesAsync(IClientSessionHandle session, CancellationToken cancellationToken = default);
}

