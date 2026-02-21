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

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<RestaurantRatingSnapshot?> GetAsync(Guid restaurantId, CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();
        var key = new RedisKey($"{KeyPrefix}{restaurantId}");

        var raw = await db.StringGetAsync(key).WaitAsync(cancellationToken);
        if (raw.IsNullOrEmpty)
        {
            return null;
        }

        RestaurantRatingCacheDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<RestaurantRatingCacheDto>(raw!, SerializerOptions);
        }
        catch
        {
            return null;
        }

        if (dto is null)
        {
            return null;
        }

        return new RestaurantRatingSnapshot(
            RestaurantId: dto.RestaurantId,
            ApprovedAverageRating: dto.ApprovedAverageRating,
            ApprovedReviewsCount: dto.ApprovedReviewsCount,
            ApprovedAverageCheck: FromCachedMoney(dto.ApprovedAverageCheck),
            ProvisionalAverageRating: dto.ProvisionalAverageRating,
            ProvisionalReviewsCount: dto.ProvisionalReviewsCount,
            ProvisionalAverageCheck: FromCachedMoney(dto.ProvisionalAverageCheck)
        );
    }

    public async Task SetAsync(RestaurantRatingSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        var db = redis.GetDatabase();

        var dto = new RestaurantRatingCacheDto(
            RestaurantId: snapshot.RestaurantId,
            ApprovedAverageRating: snapshot.ApprovedAverageRating,
            ApprovedReviewsCount: snapshot.ApprovedReviewsCount,
            ApprovedAverageCheck: ToCachedMoney(snapshot.ApprovedAverageCheck),
            ProvisionalAverageRating: snapshot.ProvisionalAverageRating,
            ProvisionalReviewsCount: snapshot.ProvisionalReviewsCount,
            ProvisionalAverageCheck: ToCachedMoney(snapshot.ProvisionalAverageCheck),
            UpdatedAtUtc: DateTimeOffset.UtcNow);

        var json = JsonSerializer.Serialize(dto, SerializerOptions);

        var key = new RedisKey($"{KeyPrefix}{snapshot.RestaurantId}");
        await db.StringSetAsync(key, json).WaitAsync(cancellationToken);
    }

    private static Money? FromCachedMoney(CachedMoney? money)
    {
        if (money is null)
        {
            return null;
        }

        try
        {
            var currency = Currency.FromCode(money.Currency);
            return new Money(money.Amount, currency);
        }
        catch
        {
            return null;
        }
    }

    private static CachedMoney? ToCachedMoney(Money? money) => money is null
                    ? null
                    : new CachedMoney(money.Value.Amount, money.Value.Currency.Code);

    private sealed record RestaurantRatingCacheDto(
        Guid RestaurantId,
        decimal ApprovedAverageRating,
        int ApprovedReviewsCount,
        CachedMoney? ApprovedAverageCheck,
        decimal ProvisionalAverageRating,
        int ProvisionalReviewsCount,
        CachedMoney? ProvisionalAverageCheck,
        DateTimeOffset UpdatedAtUtc);

    private sealed record CachedMoney(decimal Amount, string Currency);
}
