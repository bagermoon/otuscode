using RestaurantEntity = RestoRate.Restaurant.Domain.RestaurantAggregate.Restaurant;

using RestoRate.BuildingBlocks.Data.Repository;
using RestoRate.Restaurant.Infrastructure.Data;

using Ardalis.SharedKernel;

namespace RestoRate.Restaurant.Infrastructure.Repositories;

public class EfRepository<T>(RestaurantDbContext context)
    : Repository<T, RestaurantDbContext>(context)
    where T : class, IAggregateRoot
{
}
