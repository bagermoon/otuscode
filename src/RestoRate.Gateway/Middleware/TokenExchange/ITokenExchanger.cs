namespace RestoRate.Gateway.Middleware.TokenExchange;

internal interface ITokenExchanger
{
    Task<ExchangeResult> ExchangeTokenAsync(string authority, string incomingToken, string[] scopes, CancellationToken ct);
}
