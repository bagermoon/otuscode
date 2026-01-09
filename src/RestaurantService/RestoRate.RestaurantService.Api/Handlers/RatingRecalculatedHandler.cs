
using MassTransit;

using Mediator;

using RestoRate.Contracts.Rating.Events;
using RestoRate.RestaurantService.Application.UseCases.Restaurants.RatingChange;

public class RatingRecalculatedHandler(ISender sender) : IConsumer<RestaurantRatingRecalculatedEvent>
{
    public async Task Consume(ConsumeContext<RestaurantRatingRecalculatedEvent> context)
    {
        var ct = context.CancellationToken;
        var message = context.Message;
        // new RatingChangeCommand(
        //     RestaurantId: message.RestaurantId,
        //     ApprovedAverageRating: message.ApprovedAverageRating,
        //     ApprovedReviewsCount: message.ApprovedReviewsCount,
        //     ProvisionalAverageRating: message.ProvisionalAverageRating,
        //     ProvisionalReviewsCount: message.ProvisionalReviewsCount,
        //     ApprovedAverageCheck: message.ApprovedAverageCheck?.ToDomain()
        // );
    }
}