using RestoRate.ServiceDefaults;
using RestoRate.Auth.Authentication;
using RestoRate.Auth.Authorization;
using RestoRate.Gateway.Configurations;

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
    options.AddPolicy("GatewayPublicCache", builder =>
    {
        builder.Expire(TimeSpan.FromMinutes(5));
        builder.SetVaryByQuery("pageNumber", "pageSize", "searchTerm", "cuisineType", "tag");
        builder.With(c => true);
    });
});

builder.Services
    .AddServiceDiscovery()
    .AddHttpForwarderWithServiceDiscovery()
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddAuthorizationBuilder()
    .AddDefaultAuthenticationPolicy();

var app = builder.Build();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/restaurants") &&
        context.Request.Method == HttpMethods.Get)
    {
        context.Request.Headers.Remove("Cache-Control");
        context.Request.Headers.Remove("Pragma");
        context.Request.Headers.Remove("Authorization");
        context.Request.Headers.Remove("Cookie");
    }
    await next();
});

app.UseOutputCache();

// Ensure authentication/authorization run before proxying
app.UseAuthentication();
app.UseAuthorization();

// IMPORTANT: Token exchange AFTER authentication
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
