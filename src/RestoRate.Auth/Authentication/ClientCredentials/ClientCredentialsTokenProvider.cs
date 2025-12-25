using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;

namespace RestoRate.Auth.Authentication.ClientCredentials;
internal sealed class ClientCredentialsTokenProvider(
    IClientCredentialsTokenClient tokenClient,
    IOptionsMonitor<OpenIdConnectOptions> oidcOptionsMonitor,
    CachedTokenManager cachedTokenManager) : IClientCredentialsTokenProvider, IDisposable
{
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);
    private readonly IDisposable? _changeSubscription = oidcOptionsMonitor.OnChange((_, _) => cachedTokenManager.ClearAll());
    public void Dispose() => _changeSubscription?.Dispose();
    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var oidcOptions = oidcOptionsMonitor.Get(OpenIdConnectDefaults.AuthenticationScheme);

        if (string.IsNullOrWhiteSpace(oidcOptions.ClientId) || string.IsNullOrWhiteSpace(oidcOptions.ClientSecret))
            return null;

        var config = await oidcOptions.ConfigurationManager!.GetConfigurationAsync(cancellationToken);
        var tokenEndpoint = config.TokenEndpoint ?? throw new InvalidOperationException("TokenEndpoint missing");

        var cacheKey = TokenCacheKeyHelper.ClientCredentialsKey(tokenEndpoint, oidcOptions.ClientId);

        if (cachedTokenManager.TryGetToken(cacheKey, out var cachedToken))
        {
            return cachedToken;
        }

        await RefreshLock.WaitAsync(cancellationToken);
        try
        {
            if (cachedTokenManager.TryGetToken(cacheKey, out cachedToken))
            {
                return cachedToken;
            }

            var result = await tokenClient.RequestTokenAsync(
                tokenEndpoint,
                oidcOptions.ClientId,
                oidcOptions.ClientSecret,
                cancellationToken);

            if (!result.Success || string.IsNullOrWhiteSpace(result.AccessToken))
            {
                return null;
            }

            cachedTokenManager.SetToken(cacheKey, result.AccessToken, result.ExpiresInSeconds);

            return result.AccessToken;
        }
        finally
        {
            RefreshLock.Release();
        }
    }
}
