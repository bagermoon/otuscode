using MassTransit;

using Mediator;

using RestoRate.Contracts.Review.Events;
using RestoRate.RatingService.Application.Mappings;
using RestoRate.RatingService.Application.UseCases.Review.Reject;

namespace RestoRate.RatingService.Api.Consumers;

public sealed class ReviewRejectedEventConsumer(
    ISender sender)
    : IConsumer<ReviewRejectedEvent>
{
    public Task Consume(ConsumeContext<ReviewRejectedEvent> context)
    {
        var message = context.Message;

        var command = new RejectReviewCommand(
            ReviewId: message.ReviewId,
            RestaurantId: message.RestaurantId,
            Rating: message.Rating,
            AverageCheck: message.AverageCheck?.ToDomainMoney());
        return sender.Send(command, context.CancellationToken).AsTask();
    }
}