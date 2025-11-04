namespace RestoRate.Contracts.Rating.Events;

public sealed record RestaurantRatingRecalculatedEvent(
    Guid RestaurantId,
    decimal ApprovedAverage,
    int ApprovedCount,
    decimal ProvisionalAverage,
    int ProvisionalCount,
    decimal? AverageCheck);
