using MassTransit;

using Microsoft.Extensions.Logging;

using System.Linq;
using RestoRate.Contracts.Review.Events;
using RestoRate.ModerationService.Domain.Abstractions;
using Ardalis.Result;

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

            var result = _moderator.Moderate(message.Comment);

            if (result.Status == ResultStatus.Ok)
            {
                _logger.LogInformation("Отзыв {ReviewId} успешно прошел модерацию", message.ReviewId);
                await context.Publish(new ReviewModerationPassedIntegrationEvent(message.ReviewId));
                return;
            }

            if (result.Status == ResultStatus.Invalid)
            {
                var reason = string.Join(';', result.Errors);
                _logger.LogWarning("Отзыв {ReviewId} отклонен модератором. Причина: {Reason}", message.ReviewId, reason);
                await context.Publish(new ReviewModerationRejectedIntegrationEvent(message.ReviewId, reason));
                return;
            }

            // Treat other statuses as error: log and publish rejection with technical reason
            var err = result.Errors.Any() ? string.Join(';', result.Errors) : "Internal moderation error";
            _logger.LogError("Модерация отзыва {ReviewId} завершилась с ошибкой: {Error}", message.ReviewId, err);
            await context.Publish(new ReviewModerationRejectedIntegrationEvent(message.ReviewId, err));
        }
    }
}
