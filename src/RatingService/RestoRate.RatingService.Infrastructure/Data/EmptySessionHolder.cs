using MongoDB.Driver;

namespace RestoRate.RatingService.Infrastructure.Data;

internal sealed class EmptySessionHolder : ISessionHolder
{
    public ValueTask<IClientSessionHandle?> GetSessionAsync(CancellationToken cancellationToken = default)
        => ValueTask.FromResult<IClientSessionHandle?>(null);

    public Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public void Dispose()
    { }
}
