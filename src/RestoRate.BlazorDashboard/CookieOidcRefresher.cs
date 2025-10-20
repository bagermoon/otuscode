using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace RestoRate.BlazorDashboard;

// https://github.com/dotnet/aspnetcore/issues/8175
internal sealed class CookieOidcRefresher(IOptionsMonitor<OpenIdConnectOptions> oidcOptionsMonitor)
{
    private readonly OpenIdConnectProtocolValidator oidcTokenValidator = new()
    {
        // We no longer have the original nonce cookie which is deleted at the end of the authorization code flow having served its purpose.
        // Even if we had the nonce, it's likely expired. It's not intended for refresh requests. Otherwise, we'd use oidcOptions.ProtocolValidator.
        RequireNonce = false,
    };

    public async Task ValidateOrRefreshCookieAsync(CookieValidatePrincipalContext validateContext, string oidcScheme)
    {
        // Skip refresh on logout to keep tokens available for id_token_hint
        var path = validateContext.HttpContext.Request.Path.Value;
        if (string.Equals(path, "/authentication/logout", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!TryGetAccessTokenExpiration(validateContext.Properties, out var accessTokenExpiration))
        {
            return;
        }

        var oidcOptions = oidcOptionsMonitor.Get(oidcScheme);
        var now = oidcOptions.TimeProvider!.GetUtcNow();

        if (!ShouldRefresh(now, accessTokenExpiration))
        {
            return;
        }

        if (!TryGetRefreshToken(validateContext.Properties, out var refreshToken))
        {
            validateContext.RejectPrincipal();
            return;
        }

        var configuration = await GetOidcConfigurationAsync(oidcOptions, validateContext.HttpContext.RequestAborted);
        var message = await RequestTokenRefreshAsync(oidcOptions, configuration, refreshToken);
        if (message is null)
        {
            validateContext.RejectPrincipal();
            return;
        }

        var validationResult = await ValidateIdTokenAsync(oidcOptions, configuration, message);
        if (!validationResult.IsValid)
        {
            validateContext.RejectPrincipal();
            return;
        }

        ValidateOidcProtocolResponse(oidcOptions, message, validationResult);

        // Renew cookie and principal
        validateContext.ShouldRenew = true;
        validateContext.ReplacePrincipal(new ClaimsPrincipal(validationResult.ClaimsIdentity));

        var expiresAt = ComputeExpiresAt(message, now);
        var rotatedRefreshToken = string.IsNullOrEmpty(message.RefreshToken) ? refreshToken : message.RefreshToken;
        StoreTokens(validateContext.Properties, message, rotatedRefreshToken, expiresAt);
    }

    private static bool TryGetAccessTokenExpiration(AuthenticationProperties properties, out DateTimeOffset accessTokenExpiration)
    {
        var text = properties.GetTokenValue("expires_at");
        return DateTimeOffset.TryParse(text, out accessTokenExpiration);
    }

    private static bool ShouldRefresh(DateTimeOffset now, DateTimeOffset accessTokenExpiration)
    {
        // Refresh when the access token will expire within the next 3 minutes
        return now + TimeSpan.FromMinutes(3) >= accessTokenExpiration;
    }

    private static bool TryGetRefreshToken(AuthenticationProperties properties, out string refreshToken)
    {
        refreshToken = properties.GetTokenValue("refresh_token") ?? string.Empty;
        return !string.IsNullOrWhiteSpace(refreshToken);
    }

    private static async Task<OpenIdConnectConfiguration> GetOidcConfigurationAsync(OpenIdConnectOptions options, CancellationToken cancellationToken)
    {
        var configuration = await options.ConfigurationManager!.GetConfigurationAsync(cancellationToken);
        return configuration;
    }

    private static async Task<OpenIdConnectMessage?> RequestTokenRefreshAsync(OpenIdConnectOptions options, OpenIdConnectConfiguration configuration, string refreshToken)
    {
        var tokenEndpoint = configuration.TokenEndpoint ?? throw new InvalidOperationException("Cannot refresh cookie. TokenEndpoint missing!");

        using var response = await options.Backchannel.PostAsync(tokenEndpoint,
            new FormUrlEncodedContent(new Dictionary<string, string?>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = options.ClientId,
                ["client_secret"] = options.ClientSecret,
                ["scope"] = string.Join(" ", options.Scope),
                ["refresh_token"] = refreshToken,
            }));

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        return new OpenIdConnectMessage(json);
    }

    private static async Task<TokenValidationResult> ValidateIdTokenAsync(OpenIdConnectOptions options, OpenIdConnectConfiguration configuration, OpenIdConnectMessage message)
    {
        var validationParameters = options.TokenValidationParameters.Clone();
        if (options.ConfigurationManager is BaseConfigurationManager baseConfigurationManager)
        {
            validationParameters.ConfigurationManager = baseConfigurationManager;
        }
        else
        {
            validationParameters.ValidIssuer = configuration.Issuer;
            validationParameters.IssuerSigningKeys = configuration.SigningKeys;
        }

        return await options.TokenHandler.ValidateTokenAsync(message.IdToken, validationParameters);
    }

    private void ValidateOidcProtocolResponse(OpenIdConnectOptions options, OpenIdConnectMessage message, TokenValidationResult validationResult)
    {
        var validatedIdToken = JwtSecurityTokenConverter.Convert(validationResult.SecurityToken as JsonWebToken);
        // Nonce is not used in refresh_token flow
        validatedIdToken.Payload["nonce"] = null;

        oidcTokenValidator.ValidateTokenResponse(new()
        {
            ProtocolMessage = message,
            ClientId = options.ClientId,
            ValidatedIdToken = validatedIdToken,
        });
    }

    private static DateTimeOffset ComputeExpiresAt(OpenIdConnectMessage message, DateTimeOffset now)
    {
        if (int.TryParse(message.ExpiresIn, NumberStyles.Integer, CultureInfo.InvariantCulture, out var expiresInSeconds))
        {
            return now + TimeSpan.FromSeconds(expiresInSeconds);
        }

        // Fallback to access token 'exp' claim
        var handler = new JwtSecurityTokenHandler();
        var at = handler.ReadJwtToken(message.AccessToken);
        return new DateTimeOffset(at.ValidTo, TimeSpan.Zero);
    }

    private static void StoreTokens(AuthenticationProperties properties, OpenIdConnectMessage message, string refreshToken, DateTimeOffset expiresAt)
    {
        properties.StoreTokens([
            new() { Name = "access_token", Value = message.AccessToken },
            new() { Name = "id_token", Value = message.IdToken },
            new() { Name = "refresh_token", Value = refreshToken },
            new() { Name = "token_type", Value = message.TokenType },
            new() { Name = "expires_at", Value = expiresAt.ToString("o", CultureInfo.InvariantCulture) },
        ]);
    }
}
