using MongoDB.Driver;

using RestoRate.Abstractions.Persistence;

namespace RestoRate.RatingService.Infrastructure.Data;

public interface ISessionHolder : ISessionContext, IDisposable
{
    Task<IClientSessionHandle?> GetSession(CancellationToken cancellationToken = default);
}
