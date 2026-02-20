using MongoDB.Bson;
using MongoDB.Driver;

using NodaMoney;

using RestoRate.RatingService.Domain.Interfaces;
using RestoRate.RatingService.Domain.ReviewReferenceAggregate;
using RestoRate.RatingService.Infrastructure.Data;

namespace RestoRate.RatingService.Infrastructure.Repositories;

internal sealed class ReviewReferenceRepository(
    IMongoContext context)
    : BaseRepository<ReviewReference>(context), IReviewReferenceRepository
{
    public Task AddReviewReferenceAsync(ReviewReference reviewReference, CancellationToken cancellationToken = default)
    {
        Add(reviewReference);
        return Task.CompletedTask;
    }

    public Task<ReviewReference?> GetReviewReferenceByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public Task DeleteReviewReferenceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Remove(id);
        return Task.CompletedTask;
    }

    public async Task<decimal?> GetAverageRatingByRestaurantIdAsync(Guid restaurantId, bool approvedOnly = true, CancellationToken cancellationToken = default)
    {
        var filter = Builders<ReviewReference>.Filter.Eq(x => x.RestaurantId, restaurantId);
        if (approvedOnly)
        {
            filter &= Builders<ReviewReference>.Filter.Eq(x => x.IsApproved, true);
        }

        var doc = await Collection
            .Aggregate()
            .Match(filter)
            .Group(new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "avgRating", new BsonDocument("$avg", "$Rating") }
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (doc is null || !doc.TryGetValue("avgRating", out var avg) || avg.IsBsonNull)
        {
            return null;
        }

        if (avg.BsonType != BsonType.Decimal128)
        {
            return null;
        }

        return Decimal128.ToDecimal(avg.AsDecimal128);
    }

    public async Task<Money?> GetAverageCheckByRestaurantIdAsync(Guid restaurantId, bool approvedOnly = true, CancellationToken cancellationToken = default)
    {
        var filter =
            Builders<ReviewReference>.Filter.Eq(x => x.RestaurantId, restaurantId) &
            Builders<ReviewReference>.Filter.Ne(x => x.AverageCheck, null);

        if (approvedOnly)
        {
            filter &= Builders<ReviewReference>.Filter.Eq(x => x.IsApproved, true);
        }

        var doc = await Collection
            .Aggregate()
            .Match(filter)
            .Group(new BsonDocument
            {
                { "_id", "$AverageCheck.Currency" },
                { "avgAmount", new BsonDocument("$avg", "$AverageCheck.Amount") },
                { "count", new BsonDocument("$sum", 1) }
            })
            .Sort(new BsonDocument("count", -1))
            .Limit(1)
            .FirstOrDefaultAsync(cancellationToken);

        if (doc is null)
        {
            return null;
        }

        var currencyCode = doc["_id"].AsString;

        if (!doc.TryGetValue("avgAmount", out var avgAmount) || avgAmount.IsBsonNull)
        {
            return null;
        }

        var amount = avgAmount.BsonType == BsonType.Decimal128
            ? Decimal128.ToDecimal(avgAmount.AsDecimal128)
            : 0m;

        return new Money(amount, Currency.FromCode(currencyCode));
    }

    public Task UpdateReviewReferenceAsync(ReviewReference reviewReference, CancellationToken cancellationToken = default)
    {
        Update(reviewReference);
        return Task.CompletedTask;
    }
}