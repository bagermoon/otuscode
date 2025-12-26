using Mediator;

using RestoRate.Contracts.Restaurant.DTOs;

namespace RestoRate.RestaurantService.Application.UseCases.Tags;

public record ListTagsQuery : IRequest<List<TagDto>>;
