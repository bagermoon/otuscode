using Ardalis.Specification;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RestoRate.RestaurantService.Domain.RestaurantAggregate.Specifications;

public sealed class GetRestaurantsByOwnerSpec : Specification<Restaurant>
{
    public GetRestaurantsByOwnerSpec(Guid ownerId)
    {
        Query
            .Where(r => r.OwnerId == ownerId)
            .Include(r => r.Rating)
            .Include(r => r.Images)
            .Include(r => r.CuisineTypes)
            .Include(r => r.Tags)
                .ThenInclude(rt => rt.Tag);
    }
}
