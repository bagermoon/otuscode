using RestaurantEntity = RestoRate.RestaurantService.Domain.RestaurantAggregate.Restaurant;

using RestoRate.BuildingBlocks.Data.Repository;
using RestoRate.RestaurantService.Infrastructure.Data;

using Ardalis.SharedKernel;

namespace RestoRate.RestaurantService.Infrastructure.Repositories;

public class EfRepository<T>(RestaurantDbContext context)
    : Repository<T, RestaurantDbContext>(context)
    where T : class, IAggregateRoot
{
}
