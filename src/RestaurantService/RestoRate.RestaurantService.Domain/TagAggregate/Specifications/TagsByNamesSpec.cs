using Ardalis.Specification;

namespace RestoRate.RestaurantService.Domain.TagAggregate.Specifications;

public class TagsByNamesSpec : Specification<Tag>
{
    public TagsByNamesSpec(IEnumerable<string> names)
    {
        var normalizedNames = names.Select(name => name.Trim().ToLowerInvariant()).ToList();
        Query.Where(t => normalizedNames.Contains(t.NormalizedName));
    }
}
