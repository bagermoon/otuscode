using MassTransit;

using Mediator;

using RestoRate.Contracts.Review.Events;
using RestoRate.RatingService.Application.UseCases.Review.Approve;

namespace RestoRate.RatingService.Api.Handlers;

public sealed class ReviewApprovedEventHandler(
    ISender sender)
    : IConsumer<ReviewApprovedEvent>
{
    public Task Consume(ConsumeContext<ReviewApprovedEvent> context)
    {
        var command = new ApproveReviewCommand(context.Message.ReviewId);
        return sender.Send(command, context.CancellationToken).AsTask();
    }
}
