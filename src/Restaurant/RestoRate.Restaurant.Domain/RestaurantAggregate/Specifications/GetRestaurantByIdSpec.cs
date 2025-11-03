using Ardalis.Specification;

namespace RestoRate.Restaurant.Domain.RestaurantAggregate.Specifications;

public sealed class GetRestaurantByIdSpec : Specification<Restaurant>
{
    public GetRestaurantByIdSpec(int restaurantId)
    {
        Query
            .Where(r => r.Id == restaurantId);
    }
}
