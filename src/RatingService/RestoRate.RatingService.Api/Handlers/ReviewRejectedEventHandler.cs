using MassTransit;

using Mediator;

using RestoRate.Contracts.Review.Events;
using RestoRate.RatingService.Application.UseCases.Review.Reject;

namespace RestoRate.RatingService.Api.Handlers;

public sealed class ReviewRejectedEventHandler(
    ISender sender)
    : IConsumer<ReviewRejectedEvent>
{
    public Task Consume(ConsumeContext<ReviewRejectedEvent> context)
    {
        var command = new RejectReviewCommand(context.Message.ReviewId);
        return sender.Send(command, context.CancellationToken).AsTask();
    }
}
