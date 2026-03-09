using Microsoft.Extensions.Logging;

namespace RestoRate.ModerationService.Api.Consumers;

internal static partial class ReviewAddedEventConsumerLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Отзыв {ReviewId} модерирован. Approved={Approved}")]
    internal static partial void LogModerated(this ILogger logger, Guid reviewId, bool approved);
}
