using MongoDB.Driver;

namespace RestoRate.RatingService.Infrastructure.Data;

internal sealed class EmptySessionHolder : ISessionHolder
{
    public Task<IClientSessionHandle?> GetSession(CancellationToken cancellationToken = default)
        => Task.FromResult<IClientSessionHandle?>(null);

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public void Dispose()
    { }
}
