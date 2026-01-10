using Ardalis.Result;

using Mediator;

using NodaMoney;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.RatingChange;
public record RatingChangeCommand(
    Guid RestaurantId,
    decimal ApprovedAverageRating,
    int ApprovedReviewsCount,
    Money ApprovedAverageCheck,
    decimal ProvisionalAverageRating,
    int ProvisionalReviewsCount,
    Money ProvisionalAverageCheck
) : ICommand<Result>;
