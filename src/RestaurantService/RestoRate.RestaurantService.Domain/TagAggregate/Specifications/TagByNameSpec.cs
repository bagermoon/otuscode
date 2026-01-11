using Ardalis.Specification;

namespace RestoRate.RestaurantService.Domain.TagAggregate.Specifications;

public sealed class TagByNameSpec : Specification<Tag>, ISingleResultSpecification<Tag>
{
    public TagByNameSpec(string name)
    {
        var normalized = name.Trim().ToLowerInvariant();
        Query.Where(t => t.NormalizedName == normalized);
    }
}
