using RestaurantEntity = RestoRate.Restaurant.Domain.RestaurantAggregate.Restaurant;

using RestoRate.BuildingBlocks.Data.Repository;
using RestoRate.Restaurant.Infrastructure.Data;

namespace RestoRate.Restaurant.Infrastructure.Repositories;

public class RestaurantRepository(RestaurantDbContext context)
    : Repository<RestaurantEntity, RestaurantDbContext>(context)
{
}
