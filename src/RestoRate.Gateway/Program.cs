using RestoRate.ServiceDefaults;
using RestoRate.Auth.Authentication;
using RestoRate.Auth.Authorization;
using RestoRate.Gateway.Configurations;
using RestoRate.Gateway.OutputCaching;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddTokenExchange();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddOptions<KeycloakSettingsOptions>()
    .Bind(builder.Configuration.GetSection(KeycloakSettingsOptions.SectionName));

builder.AddGatewayJwtAuthentication(AppHostProjects.Keycloak);

builder.AddRedisOutputCache(AppHostProjects.RedisCache);

builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("RestaurantsAuthenticatedOutputCachePolicy", new RestaurantsAuthenticatedOutputCachePolicy(5));
});

builder.Services
    .AddServiceDiscovery()
    .AddHttpForwarderWithServiceDiscovery()
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddAuthorizationBuilder()
    .AddDefaultAuthenticationPolicy();

var app = builder.Build();

// 1)  incoming JWT first.
app.UseAuthentication();
app.UseAuthorization();

// 2) Cache only after the request has been authorized.
//    This allows cached responses to short-circuit BEFORE token exchange.
app.UseOutputCache();

// 3) Exchange token only when we actually need to proxy (cache miss).
app.UseTokenExchange();

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
}

// Enforce the policy on proxied routes
app.MapReverseProxy()
    .RequireAuthorization();

app.Run();
