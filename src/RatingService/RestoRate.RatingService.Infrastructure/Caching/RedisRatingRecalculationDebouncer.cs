using System.Globalization;

using RestoRate.RatingService.Domain.Interfaces;

using StackExchange.Redis;

namespace RestoRate.RatingService.Infrastructure.Caching;

internal sealed class RedisRatingRecalculationDebouncer(
    IConnectionMultiplexer redis)
    : IRatingRecalculationDebouncer
{
    private const string DueSetKey = "rating:recalc:due";
    private const string LockKeyPrefix = "rating:recalc:lock:";
    private static readonly LuaScript TryCompleteScript = LuaScript.Prepare(
        """
        local score = redis.call('ZSCORE', @dueSetKey, @member)
        if not score then
            return 0
        end

        if score ~= @expectedScore then
            return 0
        end

        redis.call('ZREM', @dueSetKey, @member)
        return 1
        """);

    public async Task MarkChangedAsync(
        Guid restaurantId,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();
        var dueAt = DateTimeOffset.UtcNow.Add(window).ToUnixTimeMilliseconds();

        await db.SortedSetAddAsync(
            DueSetKey,
            GetRestaurantMember(restaurantId),
            dueAt).WaitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetDueRestaurantIdsAsync(
        DateTimeOffset asOf,
        int take,
        CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();
        var members = await db.SortedSetRangeByScoreAsync(
            DueSetKey,
            stop: asOf.ToUnixTimeMilliseconds(),
            exclude: Exclude.None,
            order: Order.Ascending,
            take: take).WaitAsync(cancellationToken);

        return members
            .Select(member => Guid.TryParse(member, out var restaurantId) ? restaurantId : Guid.Empty)
            .Where(restaurantId => restaurantId != Guid.Empty)
            .ToArray();
    }

    public async Task<DateTimeOffset?> GetDueAtAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();
        var score = await db.SortedSetScoreAsync(DueSetKey, GetRestaurantMember(restaurantId)).WaitAsync(cancellationToken);

        if (score is null)
        {
            return null;
        }

        return DateTimeOffset.FromUnixTimeMilliseconds((long)score.Value);
    }

    public async Task<bool> TryAcquireProcessingLockAsync(
        Guid restaurantId,
        TimeSpan lockDuration,
        CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();

        return await db.StringSetAsync(
            GetLockKey(restaurantId),
            value: 1,
            expiry: lockDuration,
            when: When.NotExists).WaitAsync(cancellationToken);
    }

    public async Task<bool> TryCompleteAsync(
        Guid restaurantId,
        DateTimeOffset expectedDueAt,
        CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();
        var result = await db.ScriptEvaluateAsync(
            TryCompleteScript,
            new
            {
                dueSetKey = (RedisKey)DueSetKey,
                member = (RedisValue)GetRestaurantMember(restaurantId),
                expectedScore = expectedDueAt.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture)
            }).WaitAsync(cancellationToken);

        return (int)result == 1;
    }

    public Task ReleaseProcessingLockAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();
        return db.KeyDeleteAsync(GetLockKey(restaurantId)).WaitAsync(cancellationToken);
    }

    private static RedisValue GetRestaurantMember(Guid restaurantId) => restaurantId.ToString("D");

    private static RedisKey GetLockKey(Guid restaurantId) => new($"{LockKeyPrefix}{restaurantId:D}");
}
