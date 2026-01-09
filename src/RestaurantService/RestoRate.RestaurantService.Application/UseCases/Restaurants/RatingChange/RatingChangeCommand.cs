using Ardalis.Result;

using Mediator;

using RestoRate.SharedKernel.ValueObjects;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.RatingChange;
public record RatingChangeCommand(
    Guid RestaurantId,
    decimal ApprovedAverageRating,
    int ApprovedReviewsCount,
    decimal ProvisionalAverageRating,
    int ProvisionalReviewsCount,
    Money? ApprovedAverageCheck = null
) : ICommand<Result>;