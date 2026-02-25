using MassTransit;

using Mediator;

using RestoRate.Contracts.Review.Events;
using RestoRate.RatingService.Application.Mappings;
using RestoRate.RatingService.Application.UseCases.Review.Add;

namespace RestoRate.RatingService.Api.Handlers;

public sealed class ReviewAddedEventHandler(
    ISender sender)
    : IConsumer<ReviewAddedEvent>
{
    public Task Consume(ConsumeContext<ReviewAddedEvent> context)
    {
        var message = context.Message;

        var command = new AddReviewCommand(
            ReviewId: message.ReviewId,
            RestaurantId: message.RestaurantId,
            Rating: message.Rating,
            AverageCheck: message.AverageCheck?.ToDomainMoney());

        return sender.Send(command, context.CancellationToken).AsTask();
    }
}
