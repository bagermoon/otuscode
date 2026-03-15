using MongoDB.Bson;
using MongoDB.Driver;

using NodaMoney;

using RestoRate.RatingService.Domain.Interfaces;
using RestoRate.RatingService.Domain.ReviewReferenceAggregate;
using RestoRate.RatingService.Infrastructure.Data;

namespace RestoRate.RatingService.Infrastructure.Repositories;

internal sealed class ReviewReferenceRepository(
    IMongoContext context,
    ISessionHolder sessionHolder)
    : BaseRepository<ReviewReference>(context, sessionHolder), IReviewReferenceRepository
{
    private static readonly FilterDefinition<ReviewReference> NotRejectedFilter =
        Builders<ReviewReference>.Filter.Or(
            Builders<ReviewReference>.Filter.Eq(x => x.IsRejected, false),
            Builders<ReviewReference>.Filter.Exists(nameof(ReviewReference.IsRejected), false));

    public async Task<bool> TryAddAsync(
        Guid reviewId,
        Guid restaurantId,
        decimal rating,
        Money? averageCheck,
        CancellationToken cancellationToken = default)
    {
        var reviewReference = ReviewReference.Create(reviewId, restaurantId, rating, averageCheck);

        if (!await InsertDirectAsync(reviewReference, cancellationToken))
        {
            return false;
        }

        TrackPersisted(reviewReference);
        return true;
    }

    public async Task<bool> TryApproveAsync(
        Guid reviewId,
        Guid restaurantId,
        decimal rating,
        Money? averageCheck,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<ReviewReference>.Filter.And(
            Builders<ReviewReference>.Filter.Eq(x => x.Id, reviewId),
            Builders<ReviewReference>.Filter.Ne(x => x.IsApproved, true),
            Builders<ReviewReference>.Filter.Ne(x => x.IsRejected, true));

        var update = Builders<ReviewReference>.Update
            .Set(x => x.RestaurantId, restaurantId)
            .Set(x => x.Rating, rating)
            .Set(x => x.AverageCheck, averageCheck)
            .Set(x => x.IsApproved, true)
            .Set(x => x.IsRejected, false);

        if (await UpdateDirectAsync(filter, update, cancellationToken))
        {
            TrackPersisted(ReviewReference.CreateApproved(reviewId, restaurantId, rating, averageCheck));
            return true;
        }

        var reviewReference = ReviewReference.CreateApproved(reviewId, restaurantId, rating, averageCheck);
        if (await InsertDirectAsync(reviewReference, cancellationToken))
        {
            TrackPersisted(reviewReference);
            return true;
        }

        if (await UpdateDirectAsync(filter, update, cancellationToken))
        {
            TrackPersisted(ReviewReference.CreateApproved(reviewId, restaurantId, rating, averageCheck));
            return true;
        }

        return false;
    }

    public async Task<bool> TryRejectAsync(
        Guid reviewId,
        Guid restaurantId,
        decimal rating,
        Money? averageCheck,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<ReviewReference>.Filter.And(
            Builders<ReviewReference>.Filter.Eq(x => x.Id, reviewId),
            Builders<ReviewReference>.Filter.Ne(x => x.IsRejected, true));

        var update = Builders<ReviewReference>.Update
            .Set(x => x.RestaurantId, restaurantId)
            .Set(x => x.Rating, rating)
            .Set(x => x.AverageCheck, averageCheck)
            .Set(x => x.IsApproved, false)
            .Set(x => x.IsRejected, true);

        if (await UpdateDirectAsync(filter, update, cancellationToken))
        {
            TrackPersisted(ReviewReference.CreateRejected(reviewId, restaurantId, rating, averageCheck));
            return true;
        }

        var reviewReference = ReviewReference.CreateRejected(reviewId, restaurantId, rating, averageCheck);
        if (await InsertDirectAsync(reviewReference, cancellationToken))
        {
            TrackPersisted(reviewReference);
            return true;
        }

        if (await UpdateDirectAsync(filter, update, cancellationToken))
        {
            TrackPersisted(ReviewReference.CreateRejected(reviewId, restaurantId, rating, averageCheck));
            return true;
        }

        return false;
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
        var filter = Builders<ReviewReference>.Filter.Eq(x => x.RestaurantId, restaurantId) & NotRejectedFilter;
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
            Builders<ReviewReference>.Filter.Ne(x => x.AverageCheck, null) &
            NotRejectedFilter;

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

    public async Task<int> GetReviewsCountByRestaurantIdAsync(
        Guid restaurantId,
        bool approvedOnly = true,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<ReviewReference>.Filter.Eq(x => x.RestaurantId, restaurantId) & NotRejectedFilter;
        if (approvedOnly)
        {
            filter &= Builders<ReviewReference>.Filter.Eq(x => x.IsApproved, true);
        }

        var count = await Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return (int)Math.Min(int.MaxValue, count);
    }

}
