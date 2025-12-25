using RestoRate.Gateway.Middleware.TokenExchange;

namespace RestoRate.Gateway.Configurations;

public static class ExchangeConfigs
{
    public static IHostApplicationBuilder AddTokenExchange(this IHostApplicationBuilder builder)
    {
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<CachedTokenManager>();
        builder.Services.AddSingleton<ExchangeLockManager>();
        // Register typed token exchanger with a named/typed HttpClient for configuration
        builder.Services.AddHttpClient<ITokenExchanger, KeycloakTokenExchanger>(KeycloakTokenExchanger.HttpClientName, client =>
            client.Timeout = TimeSpan.FromSeconds(60)
        );

        return builder;
    }

    public static IApplicationBuilder UseTokenExchange(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        return app.UseMiddleware<TokenExchangeMiddleware>();
    }
}