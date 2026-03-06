using MassTransit;

using Mediator;

using RestoRate.Contracts.Review.Events;
using RestoRate.ReviewService.Application.UseCases.Reviews.Reject;

namespace RestoRate.ReviewService.Api.Handlers;

public sealed class ReviewModerationRejectedHandler(
    ISender sender,
    ILogger<ReviewModerationRejectedHandler> logger
) : IConsumer<ReviewModerationRejectedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ReviewModerationRejectedIntegrationEvent> context)
    {
        var reviewId = context.Message.ReviewId;
        var reason = context.Message.Reason;

        logger.LogInformation("Получен сигнал об отклонении отзыва {ReviewId}. Причина: {Reason}", reviewId, reason);

        var command = new RejectReviewCommand(reviewId);
        var result = await sender.Send(command, context.CancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Не удалось отклонить отзыв {ReviewId}: {Errors}", reviewId, string.Join(", ", result.Errors));
        }
    }
}
