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

builder.Services
    .AddServiceDiscovery()
    .AddHttpForwarderWithServiceDiscovery()
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddAuthorizationBuilder()
    .AddDefaultAuthenticationPolicy();

var app = builder.Build();

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
