using Ardalis.SharedKernel;

using RestoRate.RestaurantService.Domain.Interfaces;
using RestoRate.RestaurantService.Domain.TagAggregate;
using RestoRate.RestaurantService.Domain.TagAggregate.Specifications;

namespace RestoRate.RestaurantService.Domain.Services;
public class TagsSvc : ITagsService
{
    private readonly IRepository<Tag> _tagRepository;

    public TagsSvc(IRepository<Tag> tagRepository)
    {
        _tagRepository = tagRepository ?? throw new ArgumentNullException(nameof(tagRepository));
    }

    public async Task<List<Tag>> ConvertToTagsAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
    {
        var result = new List<Tag>();
        if (!names.Any())
        {
            return result;
        }

        var tagsToAdd = new List<Tag>();
        var uniqueTags = names
            .Select(name => name.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase);

        var spec = new TagsByNamesSpec(uniqueTags);
        var existingTags = await _tagRepository.ListAsync(spec, cancellationToken);
        result.AddRange(existingTags);

        var newTags = uniqueTags
            .Where(tagName => !existingTags.Any(
                t => t.NormalizedName.Equals(tagName, StringComparison.InvariantCultureIgnoreCase)))
            .Select(tagName => new Tag(tagName))
            .ToList();

        if (newTags.Count > 0)
        {
            result.AddRange(await _tagRepository.AddRangeAsync(newTags, cancellationToken));
        }

        return result;
    }
}