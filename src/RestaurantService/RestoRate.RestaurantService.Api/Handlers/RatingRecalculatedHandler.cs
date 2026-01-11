
using MassTransit;

using Mediator;

using NodaMoney;

using RestoRate.Contracts.Rating.Events;
using RestoRate.RestaurantService.Application.Mappings;
using RestoRate.RestaurantService.Application.UseCases.Restaurants.RatingChange;

public class RatingRecalculatedHandler(ISender sender) : IConsumer<RestaurantRatingRecalculatedEvent>
{
    public async Task Consume(ConsumeContext<RestaurantRatingRecalculatedEvent> context)
    {
        var ct = context.CancellationToken;
        var message = context.Message;

        var cmd = new RatingChangeCommand(
            RestaurantId: message.RestaurantId,
            ApprovedAverageRating: message.ApprovedAverageRating,
            ApprovedReviewsCount: message.ApprovedReviewsCount,
            ApprovedAverageCheck: message.ApprovedAverageCheck?.ToDomainMoney() ?? Money.Zero,
            ProvisionalAverageRating: message.ProvisionalAverageRating,
            ProvisionalReviewsCount: message.ProvisionalReviewsCount,
            ProvisionalAverageCheck: message.ProvisionalAverageCheck?.ToDomainMoney() ?? Money.Zero
        );

        await sender.Send(cmd, ct);
    }
}
