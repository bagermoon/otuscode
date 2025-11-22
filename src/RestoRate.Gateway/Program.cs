using RestoRate.ServiceDefaults;
using RestoRate.Gateway;
using RestoRate.Auth.Authentication;
using RestoRate.Auth.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var keycloakSettings = new KeycloakSettingsOptions();
builder.Configuration.GetSection(KeycloakSettingsOptions.SectionName).Bind(keycloakSettings);

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
app.UseMiddleware<TokenExchangeMiddleware>();

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