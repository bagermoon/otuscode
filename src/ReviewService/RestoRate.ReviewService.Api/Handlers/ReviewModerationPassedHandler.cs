using MassTransit;

using Mediator;

using RestoRate.Contracts.Review.Events;
using RestoRate.ReviewService.Application.UseCases.Reviews.Approve;

namespace RestoRate.ReviewService.Api.Handlers;

public sealed class ReviewModerationPassedHandler(
    ISender sender,
    ILogger<ReviewModerationPassedHandler> logger
) : IConsumer<ReviewModerationPassedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ReviewModerationPassedIntegrationEvent> context)
    {
        var reviewId = context.Message.ReviewId;
        logger.LogInformation("Получен сигнал об успешной модерации отзыва {ReviewId}. Одобряем...", reviewId);

        var command = new ApproveReviewCommand(reviewId);
        var result = await sender.Send(command, context.CancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Не удалось одобрить отзыв {ReviewId}: {Errors}", reviewId, string.Join(", ", result.Errors));
        }
    }
}
