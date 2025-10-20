using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace RestoRate.Gateway;

public class TokenExchangeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<JwtBearerOptions> _jwtOptions;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenExchangeMiddleware> _logger;

    public TokenExchangeMiddleware(
        RequestDelegate next,
        IHttpClientFactory httpClientFactory,
        IOptionsMonitor<JwtBearerOptions> jwtOptions,
        IConfiguration configuration,
        ILogger<TokenExchangeMiddleware> logger)
    {
        _next = next;
        _httpClientFactory = httpClientFactory;
        _jwtOptions = jwtOptions;
        _configuration = configuration;
        _logger = logger;
    }

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

        try
        {
            // 2) Build token exchange request
            var options = _jwtOptions.Get(JwtBearerDefaults.AuthenticationScheme);
            using var tokenClient = _httpClientFactory.CreateClient();

            var realm = _configuration["KeycloakSettings:Realm"]!;
            var clientId = _configuration["KeycloakSettings:ClientId"]!;
            var clientSecret = _configuration["KeycloakSettings:ClientSecret"]!;

            using var tokenRequest = new HttpRequestMessage(HttpMethod.Post,
                $"{options.Authority}/protocol/openid-connect/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "urn:ietf:params:oauth:grant-type:token-exchange",
                    ["subject_token"] = incomingToken,
                    ["subject_token_type"] = "urn:ietf:params:oauth:token-type:access_token",
                    ["requested_token_type"] = "urn:ietf:params:oauth:token-type:access_token",
                    ["client_id"] = clientId,
                    ["client_secret"] = clientSecret
                })
            };

            // 3) Perform exchange
            using var tokenResponse = await tokenClient.SendAsync(tokenRequest, context.RequestAborted);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Token exchange failed with status {StatusCode}", tokenResponse.StatusCode);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Token exchange failed");
                return; // Short-circuit the pipeline
            }

            var payload = await tokenResponse.Content.ReadAsStringAsync(context.RequestAborted);
            var accessToken = JsonDocument.Parse(payload).RootElement.GetProperty("access_token").GetString();

            if (string.IsNullOrEmpty(accessToken))
            {
                _logger.LogWarning("Token exchange succeeded but no access token returned");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid token exchange response");
                return; // Short-circuit the pipeline
            }

            // 4) Replace the Authorization header with the new token
            context.Request.Headers.Remove("Authorization");
            context.Request.Headers.Authorization = $"Bearer {accessToken}";

            _logger.LogDebug("Token exchange successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token exchange");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal server error during token exchange");
            return;
        }

        // Continue to the next middleware
        await _next(context);
    }
}
