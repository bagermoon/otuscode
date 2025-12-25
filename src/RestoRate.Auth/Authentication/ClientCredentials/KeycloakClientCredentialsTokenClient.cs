using System.Text.Json;

namespace RestoRate.Auth.Authentication.ClientCredentials;

internal sealed class KeycloakClientCredentialsTokenClient(HttpClient httpClient) : IClientCredentialsTokenClient
{
    public const string HttpClientName = "KeycloakClientCredentials";

    public async Task<ClientCredentialsTokenResult> RequestTokenAsync(
        string tokenEndpoint,
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
            })
        };

        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return ClientCredentialsTokenResult.Failed($"Token endpoint returned {response.StatusCode}");
        }

        var payload = await response.Content.ReadAsStringAsync(cancellationToken);
        using var json = JsonDocument.Parse(payload);

        if (!json.RootElement.TryGetProperty("access_token", out var accessTokenElement))
        {
            return ClientCredentialsTokenResult.Failed("No access_token in response");
        }

        var accessToken = accessTokenElement.GetString();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return ClientCredentialsTokenResult.Failed("Empty access_token in response");
        }

        var expiresInSeconds = 60;
        if (json.RootElement.TryGetProperty("expires_in", out var expiresInElement) &&
            expiresInElement.TryGetInt32(out var parsedExpiresIn) && parsedExpiresIn > 0)
        {
            expiresInSeconds = parsedExpiresIn;
        }

        return ClientCredentialsTokenResult.Succeeded(accessToken, expiresInSeconds);
    }
}
