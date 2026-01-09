using Ardalis.SharedKernel;

using Mediator;

using RestoRate.Contracts.Common.Dtos;
using RestoRate.RestaurantService.Domain.TagAggregate;
using RestoRate.RestaurantService.Domain.TagAggregate.Specifications;

namespace RestoRate.RestaurantService.Application.UseCases.Tags;

public class ListTagsQueryHandler : IRequestHandler<ListTagsQuery, List<TagDto>>
{
    private readonly IReadRepository<Tag> _repository;

    public ListTagsQueryHandler(IReadRepository<Tag> repository)
    {
        _repository = repository;
    }

    public async ValueTask<List<TagDto>> Handle(ListTagsQuery request, CancellationToken cancellationToken)
    {
        var spec = new TagListSpec();

        var tags = await _repository.ListAsync(spec, cancellationToken);

        return tags.Select(t => new TagDto(t.Id, t.Name)).ToList();
    }
}
