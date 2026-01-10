
using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using Microsoft.Extensions.Logging;

using RestoRate.RestaurantService.Domain.RestaurantAggregate;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.RatingChange;

public sealed class RatingChangeHandler(
    IRepository<Restaurant> restaurantRepository,
    ILogger<RatingChangeHandler> logger
) : ICommandHandler<RatingChangeCommand, Result>
{
    public async ValueTask<Result> Handle(RatingChangeCommand command, CancellationToken cancellationToken)
    {
        var restaurant = await restaurantRepository.GetByIdAsync(command.RestaurantId, cancellationToken);

        if (restaurant == null)
        {
            logger.LogRestaurantNotFound(command.RestaurantId);
            return Result.NotFound();
        }

        restaurant.UpdateRatings(
            command.ApprovedAverageRating,
            command.ApprovedReviewsCount,
            command.ApprovedAverageCheck,
            command.ProvisionalAverageRating,
            command.ProvisionalReviewsCount,
            command.ProvisionalAverageCheck);

        try
        {
            await restaurantRepository.UpdateAsync(restaurant, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogRatingUpdateFailed(ex, command.RestaurantId, ex.Message);
            return Result.Error($"Ошибка при обновлении рейтинга ресторана: {ex.Message}");
        }
        
        return Result.NoContent();
    }
}