
using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using Microsoft.Extensions.Logging;

using RestoRate.RestaurantService.Domain.RestaurantAggregate;
using RestoRate.RestaurantService.Domain.RestaurantAggregate.Specifications;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.RatingChange;

public sealed class RatingChangeHandler(
    IRepository<Restaurant> restaurantRepository,
    ILogger<RatingChangeHandler> logger
) : ICommandHandler<RatingChangeCommand, Result>
{
    public async ValueTask<Result> Handle(RatingChangeCommand command, CancellationToken cancellationToken)
    {
        var spec = new GetRestaurantByIdSpec(command.RestaurantId);
        var restaurant = await restaurantRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (restaurant == null)
        {
            logger.LogRestaurantNotFound(command.RestaurantId);
            return Result.NotFound();
        }

        var result = restaurant.UpdateRatings(
            command.ApprovedAverageRating,
            command.ApprovedReviewsCount,
            command.ApprovedAverageCheck,
            command.ProvisionalAverageRating,
            command.ProvisionalReviewsCount,
            command.ProvisionalAverageCheck);

        if (result.IsError())
        {
            logger.LogRatingUpdateFailed(command.RestaurantId, result.Errors.First());
            return Result.Error(result.Errors.First());
        }

        try
        {
            await restaurantRepository.UpdateAsync(restaurant, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogRatingUpdateFailed(command.RestaurantId, ex.Message, ex);
            return Result.Error($"Ошибка при обновлении рейтинга ресторана: {ex.Message}");
        }

        return Result.NoContent();
    }
}
