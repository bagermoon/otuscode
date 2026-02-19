namespace RestoRate.RatingService.Infrastructure.Data
{
    using Microsoft.Extensions.Logging;

    internal static partial class MongoUnitOfWorkLogger
    {
        [LoggerMessage(Level = LogLevel.Error, Message = "Failed to dispatch domain events after Mongo save.")]
        internal static partial void LogDomainEventsDispatchFailed(this ILogger logger, Exception exception);
    }
}