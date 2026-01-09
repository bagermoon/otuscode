using Mediator;

using RestoRate.Contracts.Common.Dtos;

namespace RestoRate.RestaurantService.Application.UseCases.Tags;

public record ListTagsQuery : IRequest<List<TagDto>>;
