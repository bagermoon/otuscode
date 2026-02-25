using MongoDB.Driver;

namespace RestoRate.RatingService.Infrastructure.Configuration;

public interface IMongoCollectionConfiguration<T>
{
    void Configure(IMongoCollection<T> collection);
}
