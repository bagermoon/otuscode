namespace RestoRate.Gateway.Middleware.TokenExchange;

internal static partial class TokenExchangeMiddlewareLogger
{
    [LoggerMessage(Level = LogLevel.Warning, Message = "Token exchange failed with status {StatusCode}")]
    internal static partial void LogTokenExchangeFailed(this ILogger logger, int statusCode);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Token exchange succeeded but no access token returned")]
    internal static partial void LogTokenExchangeNoAccessToken(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error during token exchange: {Message}")]
    internal static partial void LogTokenExchangeError(this ILogger logger, Exception ex, string message);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Token exchange successful")]
    internal static partial void LogTokenExchangeSuccessful(this ILogger logger);
}
