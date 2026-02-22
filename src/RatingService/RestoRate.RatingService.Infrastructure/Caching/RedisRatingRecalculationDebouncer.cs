using RestoRate.RatingService.Domain.Interfaces;

using StackExchange.Redis;

namespace RestoRate.RatingService.Infrastructure.Caching;

internal sealed class RedisRatingRecalculationDebouncer(
    IConnectionMultiplexer redis)
    : IRatingRecalculationDebouncer
{
    private const string WindowKeyPrefix = "rating:recalc:window:";

    public async Task<bool> TryEnterWindowAsync(
        Guid restaurantId,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();
        var key = new RedisKey($"{WindowKeyPrefix}{restaurantId}");

        return await db
            .StringSetAsync(key, value: 1, expiry: window, when: When.NotExists)
            .WaitAsync(cancellationToken);
    }
}
