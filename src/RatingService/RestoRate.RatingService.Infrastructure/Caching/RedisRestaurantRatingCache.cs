using System.Text.Json;
using System.Text.Json.Serialization;

using NodaMoney;

using RestoRate.RatingService.Domain.Interfaces;
using RestoRate.RatingService.Domain.Models;

using StackExchange.Redis;

namespace RestoRate.RatingService.Infrastructure.Caching;

internal sealed class RedisRestaurantRatingCache(
    IConnectionMultiplexer redis)
    : IRestaurantRatingCache
{
    private const string KeyPrefix = "rating:restaurant:";
    private const string ApprovedSuffix = ":approved";
    private const string ProvisionalSuffix = ":provisional";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<RestaurantRatingSnapshot?> GetAsync(
        Guid restaurantId,
        bool approvedOnly,
        CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();
        var key = BuildKey(restaurantId, approvedOnly);

        var raw = await db.StringGetAsync(key).WaitAsync(cancellationToken);
        if (raw.IsNullOrEmpty)
        {
            return null;
        }

        RestaurantRatingSnapshotCacheDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<RestaurantRatingSnapshotCacheDto>(raw!, SerializerOptions);
        }
        catch
        {
            return null;
        }

        if (dto is null)
        {
            return null;
        }

        var averageCheck = FromCachedMoney(dto.AverageCheck);

        return new RestaurantRatingSnapshot(
            RestaurantId: dto.RestaurantId,
            AverageRating: dto.AverageRating,
            ReviewsCount: dto.ReviewsCount,
            AverageCheck: averageCheck);
    }

    public async Task SetAsync(
        RestaurantRatingSnapshot snapshot,
        bool approvedOnly,
        CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();

        var dto = new RestaurantRatingSnapshotCacheDto(
            RestaurantId: snapshot.RestaurantId,
            AverageRating: snapshot.AverageRating,
            ReviewsCount: snapshot.ReviewsCount,
            AverageCheck: ToCachedMoney(snapshot.AverageCheck),
            UpdatedAtUtc: DateTimeOffset.UtcNow);

        var json = JsonSerializer.Serialize(dto, SerializerOptions);

        var key = BuildKey(snapshot.RestaurantId, approvedOnly);
        await db.StringSetAsync(key, json).WaitAsync(cancellationToken);
    }

    private static RedisKey BuildKey(Guid restaurantId, bool approvedOnly)
        => new($"{KeyPrefix}{restaurantId}{(approvedOnly ? ApprovedSuffix : ProvisionalSuffix)}");

    private static Money FromCachedMoney(CachedMoney money)
    {
        try
        {
            var currency = Currency.FromCode(money.Currency);
            return new Money(money.Amount, currency);
        }
        catch
        {
            return Money.Zero;
        }
    }

    private static CachedMoney ToCachedMoney(Money money)
        => new(money.Amount, money.Currency.Code);

    private sealed record RestaurantRatingSnapshotCacheDto(
        Guid RestaurantId,
        decimal AverageRating,
        int ReviewsCount,
        CachedMoney AverageCheck,
        DateTimeOffset UpdatedAtUtc);

    private sealed record CachedMoney(decimal Amount, string Currency);
}
