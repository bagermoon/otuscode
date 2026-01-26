using Microsoft.Extensions.Logging;

namespace RestoRate.ReviewService.Application.UseCases.UserReferences.UpsertUser;

internal static partial class UpsertUserHandlerLogger
{
    [LoggerMessage(Level = LogLevel.Information, Message = "UserReference уже существует для {UserId}; возвращается текущее сохраненное значение.")]
    internal static partial void LogUserReferenceAlreadyExists(this ILogger logger, Guid userId);
}
