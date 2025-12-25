using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Authentication.JwtBearer;
// no direct memory cache usage; cached logic moved to CachedTokenManager
using Microsoft.Extensions.Options;

using RestoRate.Auth.Authentication;

namespace RestoRate.Gateway.Middleware.TokenExchange;

internal sealed class TokenExchangeMiddleware : IDisposable
{
    private readonly RequestDelegate _next;
    private readonly IOptionsMonitor<JwtBearerOptions> _jwtOptions;
    private readonly ITokenExchanger _tokenExchanger;
    private readonly ILogger<TokenExchangeMiddleware> _logger;
    private readonly CachedTokenManager _cachedTokenManager;
    private readonly ExchangeLockManager _exchangeLockManager;
    private readonly IDisposable? _settingsChangeListener;

    public TokenExchangeMiddleware(
        RequestDelegate next,
        IOptionsMonitor<JwtBearerOptions> jwtOptions,
        ITokenExchanger tokenExchanger,
        ILogger<TokenExchangeMiddleware> logger,
        CachedTokenManager cachedTokenManager,
        ExchangeLockManager exchangeLockManager,
        IOptionsMonitor<KeycloakSettingsOptions> kSettings
        )
    {
        _next = next;
        _jwtOptions = jwtOptions;
        _tokenExchanger = tokenExchanger;
        _logger = logger;
        _cachedTokenManager = cachedTokenManager;
        _exchangeLockManager = exchangeLockManager;
        _settingsChangeListener = kSettings.OnChange((_, __) => cachedTokenManager.ClearAll());
    }
    public void Dispose() => _settingsChangeListener?.Dispose();
    public async Task InvokeAsync(HttpContext context)
    {
        // 1) Extract incoming bearer token
        var authHeader = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authHeader) ||
            !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            // No token or not a bearer token - let it flow through normally
            await _next(context);
            return;
        }

        var incomingToken = authHeader["Bearer ".Length..].Trim();
        var options = _jwtOptions.Get(JwtBearerDefaults.AuthenticationScheme);
        var authority = options.Authority ?? throw new InvalidOperationException("Authority must be configured for token exchange.");
        var scope = JwtRegisteredClaimNames.Email;
        var cacheKey = CachedTokenManager.BuildCacheKey(scope, incomingToken);

        if (_cachedTokenManager.TryGetToken(cacheKey, out var cachedAccessToken))
        {
            context.Request.Headers.Authorization = $"Bearer {cachedAccessToken}";
            await _next(context);
            return;
        }

        await _exchangeLockManager.WaitAsync(cacheKey, context.RequestAborted);
        try
        {
            if (_cachedTokenManager.TryGetToken(cacheKey, out cachedAccessToken))
            {
                context.Request.Headers.Authorization = $"Bearer {cachedAccessToken}";
                await _next(context);
                return;
            }

            try
            {
                // 2) Request token exchange
                var exchangeResult = await _tokenExchanger.ExchangeTokenAsync(
                        authority,
                        incomingToken,
                        new[] { scope },
                        context.RequestAborted
                    );

                if (exchangeResult.ErrorMessage == "no_access_token")
                {
                    _logger.LogTokenExchangeNoAccessToken();
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Invalid token exchange response");
                    return; // Short-circuit the pipeline
                }

                if (!exchangeResult.Success)
                {
                    _logger.LogTokenExchangeNoAccessToken();
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Invalid token exchange response");
                    return; // Short-circuit the pipeline
                }

                var accessToken = exchangeResult.AccessToken;
                _cachedTokenManager.SetToken(cacheKey, accessToken!, exchangeResult.ExpiresIn);

                // 3) Replace the Authorization header with the new token
                context.Request.Headers.Remove("Authorization");
                context.Request.Headers.Authorization = $"Bearer {accessToken}";

                _logger.LogTokenExchangeSuccessful();
            }
            catch (Exception ex)
            {
                _logger.LogTokenExchangeError(ex, ex.Message);
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal server error during token exchange");
                return;
            }

            // Continue to the next middleware
            await _next(context);
        }
        finally
        {
            _exchangeLockManager.Release(cacheKey);
        }
    }
}
