using MongoDB.Driver;

namespace RestoRate.RatingService.Infrastructure.Data;

public interface IMongoCollectionProvider
{
    IMongoCollection<T> GetRequiredCollection<T>() where T : class;
}
