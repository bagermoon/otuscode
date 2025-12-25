using Microsoft.Extensions.Caching.Memory;

namespace RestoRate.Gateway.Middleware.TokenExchange;

internal sealed class ExchangeLockManager(IMemoryCache cache)
{
    private sealed class LockEntry
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);

        public int RefCount { get; set; }
    }

    private readonly IMemoryCache _cache = cache;
    private readonly object _gate = new();

    private static string GetLockKey(string cacheKey) => $"{cacheKey}:exchange-lock";

    public async Task WaitAsync(string cacheKey, CancellationToken cancellationToken)
    {
        var lockKey = GetLockKey(cacheKey);
        LockEntry entry;

        lock (_gate)
        {
            entry = _cache.GetOrCreate(lockKey, static _ => new LockEntry())!;
            entry.RefCount++;
        }

        try
        {
            await entry.Semaphore.WaitAsync(cancellationToken);
        }
        catch
        {
            lock (_gate)
            {
                entry.RefCount = Math.Max(0, entry.RefCount - 1);
                if (entry.RefCount == 0)
                {
                    _cache.Remove(lockKey);
                }
            }

            throw;
        }
    }

    public void Release(string cacheKey)
    {
        var lockKey = GetLockKey(cacheKey);
        LockEntry? entry;

        lock (_gate)
        {
            if (!_cache.TryGetValue(lockKey, out entry) || entry is null)
            {
                return;
            }

            entry.RefCount = Math.Max(0, entry.RefCount - 1);
        }

        entry.Semaphore.Release();

        lock (_gate)
        {
            if (entry.RefCount == 0)
            {
                _cache.Remove(lockKey);
            }
        }
    }
}
