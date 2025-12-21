using System.Diagnostics;
using System.Reflection;
using Mediator;

using Ardalis.GuardClauses;

using Microsoft.Extensions.Logging;

namespace RestoRate.Abstractions.Mediation.Behaviors;

public class RequestLoggingBehavior<TMessage, TResponse>(ILogger<RequestLoggingBehavior<TMessage, TResponse>> logger) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IMessage
{
    private readonly ILogger<RequestLoggingBehavior<TMessage, TResponse>> _logger = logger;

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(message);
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogHandlingRequestReport(typeof(TMessage).Name);

            Type myType = message.GetType();

            IList<PropertyInfo> props = [.. myType.GetProperties()];
            foreach (PropertyInfo prop in props)
            {
                object? propValue = prop?.GetValue(message, null);
                _logger.LogHandlingRequestReportProperty(prop?.Name, propValue);
            }
        }

        var sw = Stopwatch.StartNew();

        var response = await next(message, cancellationToken);

        _logger.LogHandlingRequestReportTime(typeof(TMessage).Name, response, sw.ElapsedMilliseconds);
        sw.Stop();

        return response;
    }
}
