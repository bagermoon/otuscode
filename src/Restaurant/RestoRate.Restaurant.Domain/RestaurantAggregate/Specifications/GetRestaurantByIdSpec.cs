using Ardalis.Specification;

namespace RestoRate.Restaurant.Domain.RestaurantAggregate.Specifications;

public sealed class GetRestaurantByIdSpec : Specification<Restaurant>
{
    public GetRestaurantByIdSpec(Guid restaurantId)
    {
        Query
            .Where(r => r.Id == restaurantId)
            .Include(r => r.Images)
            .Include(r => r.CuisineTypes)
            .Include(r => r.Tags)
                .ThenInclude(rt => rt.Tag);
    }
}
