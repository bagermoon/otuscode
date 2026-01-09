using System;

using RestoRate.RestaurantService.Domain.TagAggregate;

namespace RestoRate.RestaurantService.Domain.Interfaces;

public interface ITagsService
{
    Task<List<Tag>> ConvertToTagsAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);
}
