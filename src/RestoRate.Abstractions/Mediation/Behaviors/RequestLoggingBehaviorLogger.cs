using Microsoft.Extensions.Logging;

using RestoRate.SharedKernel.Diagnostics;

namespace RestoRate.Abstractions.Mediation.Behaviors;

internal static partial class RequestLoggingBehaviorLogger
{
    [LoggerMessage(
            EventId = LoggingEventIds.HandlingRequestReport, Level = LogLevel.Information,
            Message = "Handling {RequestName}")]
    public static partial void LogHandlingRequestReport(this ILogger logger, string requestName);
    [LoggerMessage(
            EventId = LoggingEventIds.HandlingRequestReportProperty, Level = LogLevel.Information,
            Message = "Property {Property} : {@Value}")]
    public static partial void LogHandlingRequestReportProperty(this ILogger logger, string? property, object? value);
    [LoggerMessage(
            EventId = LoggingEventIds.HandlingRequestReportTime, Level = LogLevel.Information,
            Message = "Handled {RequestName} with {Response} in {ms} ms")]
    public static partial void LogHandlingRequestReportTime(this ILogger logger, string requestName, object? response, long ms);
}
