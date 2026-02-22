using Ardalis.Result;

using Mediator;

using RestoRate.Contracts.Rating.Dtos;
using RestoRate.RatingService.Application.Mappings;
using RestoRate.RatingService.Application.Services;

namespace RestoRate.RatingService.Application.UseCases.Ratings.GetById;

public sealed class GetRatingByIdHandler(
    RatingProviderService ratingProvider)
    : IQueryHandler<GetRatingByIdQuery, Result<RestaurantRatingDto>>
{
    public async ValueTask<Result<RestaurantRatingDto>> Handle(GetRatingByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await ratingProvider.GetRatingAsync(request.Id, cancellationToken);

        if (result is null)
        {
            return Result<RestaurantRatingDto>.NotFound();
        }

        return result.ToDto();
    }
}
