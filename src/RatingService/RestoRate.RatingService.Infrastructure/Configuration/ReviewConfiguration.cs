using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

using RestoRate.BuildingBlocks.Serialization;
using RestoRate.RatingService.Domain.ReviewReferenceAggregate;

namespace RestoRate.RatingService.Infrastructure.Configuration;
internal sealed class ReviewConfiguration : IMongoCollectionConfiguration<ReviewReference>
{
    public void Configure(IMongoCollection<ReviewReference> collection)
    {
        MoneyBsonSerializer.EnsureRegistered();

        if (!BsonClassMap.IsClassMapRegistered(typeof(ReviewReference)))
        {
            BsonClassMap.RegisterClassMap<ReviewReference>(cfg =>
            {
                cfg.AutoMap();
                cfg.MapIdProperty(review => review.Id);
                cfg.MapMember(review => review.Rating)
                    .SetSerializer(new DecimalSerializer(BsonType.Decimal128));
            });
        }

        var indexKeysDefinition = Builders<ReviewReference>.IndexKeys.Ascending(x => x.RestaurantId);
        const string indexName = "ix_review_reference_restaurant_id";

        if (!IsIndexExists(collection, indexName))
        {
            var indexModel = new CreateIndexModel<ReviewReference>(
                indexKeysDefinition,
                new CreateIndexOptions { Name = indexName });
            collection.Indexes.CreateOne(indexModel);
        }
    }

    private static bool IsIndexExists(IMongoCollection<ReviewReference> collection, string indexName)
    {
        var existingIndexes = collection.Indexes.List().ToList();
        return existingIndexes.Any(i =>
            i.TryGetValue("name", out var name) && name == (BsonValue)indexName);
    }
}
