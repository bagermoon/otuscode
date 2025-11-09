using RestaurantEntity = RestoRate.Restaurant.Domain.RestaurantAggregate.Restaurant;

using RestoRate.Restaurant.Infrastructure.Data;
using RestoRate.BuildingBlocks.Data.Repository;

namespace RestoRate.Restaurant.Infrastructure.Repositories;

public class RestaurantReadRepository(RestaurantDbContext context)
    : ReadRepository<RestaurantEntity, RestaurantDbContext>(context)
{
}
