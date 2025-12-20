using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ardalis.Specification;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        if (!string.IsNullOrWhiteSpace(searchTerm))
            Query.Search(r => r.Name, $"%{searchTerm}%");

        if (!string.IsNullOrWhiteSpace(cuisineType))
            Query.Where(r => r.CuisineTypes.Any(ct => ct.CuisineType.Name == cuisineType));

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var normalizedTag = tag.Trim().ToLower();
            Query.Where(r => r.Tags.Any(t => t.Tag.Name.Equals(normalizedTag, StringComparison.InvariantCultureIgnoreCase)));
        }
    }
}
