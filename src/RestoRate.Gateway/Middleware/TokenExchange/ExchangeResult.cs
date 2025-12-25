using System.Net;

namespace RestoRate.Gateway.Middleware.TokenExchange;

internal readonly record struct ExchangeResult(
    HttpStatusCode StatusCode,
    string? AccessToken = default,
    int? ExpiresIn = default,
    string? ErrorMessage = default)
{
    public bool Success => StatusCode == HttpStatusCode.OK;
}
