
using Microsoft.AspNetCore.Authentication.JwtBearer;

using RestoRate.ServiceDefaults;
using RestoRate.Gateway;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var keycloakSettings = new KeycloakSettingsOptions();
builder.Configuration.GetSection(KeycloakSettingsOptions.SectionName).Bind(keycloakSettings);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddKeycloakJwtBearer(
        serviceName: AppHostProjects.Keycloak,
        realm: keycloakSettings.Realm!,
        options =>
        {
            options.RequireHttpsMetadata = false; // dev with http Keycloak

            options.Audience = keycloakSettings.Audience;
            options.TokenValidationParameters.ValidateIssuer = false; // for service to service calls we don't validate the issuer
        }
    );

builder.Services
    .AddServiceDiscovery()
    .AddHttpForwarderWithServiceDiscovery()
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("authenticated", policy => policy.RequireAuthenticatedUser());

var app = builder.Build();

// Ensure authentication/authorization run before proxying
app.UseAuthentication();
app.UseAuthorization();

// IMPORTANT: Token exchange AFTER authentication
app.UseMiddleware<TokenExchangeMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Enforce the policy on proxied routes
app.MapReverseProxy()
    .RequireAuthorization("authenticated");

app.Run();