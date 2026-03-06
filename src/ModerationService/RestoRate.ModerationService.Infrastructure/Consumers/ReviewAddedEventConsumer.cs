using MassTransit;

using Microsoft.Extensions.Logging;

using RestoRate.Contracts.Review.Events;
using RestoRate.ModerationService.Application.Interfaces;

namespace RestoRate.ModerationService.Infrastructure.Consumers
{
    public class ReviewAddedEventConsumer : IConsumer<ReviewAddedEvent>
    {
        private readonly ITextModerator _moderator;
        private readonly ILogger<ReviewAddedEventConsumer> _logger;

        public ReviewAddedEventConsumer(
            ITextModerator moderator,
            ILogger<ReviewAddedEventConsumer> logger)
        {
            _moderator = moderator;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ReviewAddedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Начата модерация отзыва {ReviewId}", message.ReviewId);

            var (isApproved, reason) = _moderator.Moderate(message.Comment);

            if (isApproved)
            {
                _logger.LogInformation("Отзыв {ReviewId} успешно прошел модерацию", message.ReviewId);

                await context.Publish(new ReviewModerationPassedIntegrationEvent(message.ReviewId));
            }
            else
            {
                _logger.LogWarning("Отзыв {ReviewId} отклонен модератором. Причина: {Reason}", message.ReviewId, reason);

                await context.Publish(new ReviewModerationRejectedIntegrationEvent(message.ReviewId, reason!));
            }
        }
    }
}
