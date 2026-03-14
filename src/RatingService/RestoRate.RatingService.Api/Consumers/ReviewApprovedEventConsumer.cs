using MassTransit;

using Mediator;

using RestoRate.Contracts.Review.Events;
using RestoRate.RatingService.Application.Mappings;
using RestoRate.RatingService.Application.UseCases.Review.Approve;

namespace RestoRate.RatingService.Api.Consumers;

public sealed class ReviewApprovedEventConsumer(
    ISender sender)
    : IConsumer<ReviewApprovedEvent>
{
    public Task Consume(ConsumeContext<ReviewApprovedEvent> context)
    {
        var message = context.Message;

        var command = new ApproveReviewCommand(
            ReviewId: message.ReviewId,
            RestaurantId: message.RestaurantId,
            Rating: message.Rating,
            AverageCheck: message.AverageCheck?.ToDomainMoney());
        return sender.Send(command, context.CancellationToken).AsTask();
    }
}