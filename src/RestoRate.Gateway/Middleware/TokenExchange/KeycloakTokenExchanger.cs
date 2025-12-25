using System.Net;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Options;

using RestoRate.Auth.Authentication;

namespace RestoRate.Gateway.Middleware.TokenExchange;

internal sealed class KeycloakTokenExchanger(
    HttpClient httpClient,
    IOptionsMonitor<KeycloakSettingsOptions> kSettings
) : ITokenExchanger
{
    internal const string HttpClientName = "KeycloakTokenExchange";
    private readonly HttpClient _httpClient = httpClient;
    private readonly IOptionsMonitor<KeycloakSettingsOptions> _kSettings = kSettings;
    public async Task<ExchangeResult> ExchangeTokenAsync(string authority, string incomingToken, string[] scopes, CancellationToken ct)
    {
        var clientId = _kSettings.CurrentValue.ClientId!;
        var clientSecret = _kSettings.CurrentValue.ClientSecret!;

        using var tokenRequest = new HttpRequestMessage(HttpMethod.Post,
            $"{authority}/protocol/openid-connect/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:token-exchange",
                ["subject_token"] = incomingToken,
                ["subject_token_type"] = "urn:ietf:params:oauth:token-type:access_token",
                ["requested_token_type"] = "urn:ietf:params:oauth:token-type:access_token",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["scope"] = string.Join(" ", scopes)
            })
        };

        using var tokenResponse = await _httpClient.SendAsync(tokenRequest, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!tokenResponse.IsSuccessStatusCode)
        {
            return new ExchangeResult(StatusCode: tokenResponse.StatusCode);
        }

        var payload = await tokenResponse.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(payload);

        if (!doc.RootElement.TryGetProperty("access_token", out var accessTokenElement)
            || string.IsNullOrWhiteSpace(accessTokenElement.GetString()))
        {
            return new ExchangeResult(StatusCode: tokenResponse.StatusCode, ErrorMessage: "no_access_token");
        }

        var accessToken = accessTokenElement.GetString();

        int? expiresIn = null;
        if (doc.RootElement.TryGetProperty("expires_in", out var expiresInElement)
            && expiresInElement.TryGetInt32(out var parsedExpiresIn)
            && parsedExpiresIn > 0)
        {
            expiresIn = parsedExpiresIn;
        }

        return new ExchangeResult(StatusCode: HttpStatusCode.OK, AccessToken: accessToken, ExpiresIn: expiresIn);
    }
}
