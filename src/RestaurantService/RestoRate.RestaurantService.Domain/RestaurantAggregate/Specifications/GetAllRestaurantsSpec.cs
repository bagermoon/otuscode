using Ardalis.Specification;

using Microsoft.EntityFrameworkCore;

using RestoRate.SharedKernel.Enums;

namespace RestoRate.RestaurantService.Domain.RestaurantAggregate.Specifications;

public sealed class GetAllRestaurantsSpec : Specification<Restaurant>
{
    public GetAllRestaurantsSpec(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        string? cuisineType = null,
        string? tag = null)
    {
        Query
            .Include(r => r.Images)
            .Include(r => r.CuisineTypes)
            .Include(r => r.Tags)
                .ThenInclude(rt => rt.Tag)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .OrderBy(r => r.Name);

#pragma warning disable CA1862 // EF query does not support string comparisons directly
        if (!string.IsNullOrWhiteSpace(searchTerm))
            Query.Where(r => r.Name.ToLower().Contains(searchTerm.Trim().ToLower()));
#pragma warning restore CA1862

        if (!string.IsNullOrWhiteSpace(cuisineType))
            if (CuisineType.TryFromName(cuisineType, true, out var cuisineEnum))
                Query.Where(r => r.CuisineTypes.Any(ct => ct.CuisineType.Equals(cuisineEnum)));

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var normalizedTag = tag.Trim().ToLower();
            Query.Where(r => r.Tags.Any(t => t.Tag.NormalizedName == normalizedTag));
        }
    }
}
