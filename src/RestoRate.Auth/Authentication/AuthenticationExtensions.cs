using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Ardalis.GuardClauses;

namespace RestoRate.Auth.Authentication;

public static class AuthenticationExtensions
{
    public static TBuilder AddGatewayJwtAuthentication<TBuilder>(
        this TBuilder builder,
        string keycloakServiceName) where TBuilder : IHostApplicationBuilder
    {
        var settings = builder.Configuration
            .GetSection(KeycloakSettingsOptions.SectionName)
            .Get<KeycloakSettingsOptions>();

        Guard.Against.Null(settings, nameof(settings));
        builder.Services
            .AddServiceDiscovery()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddKeycloakJwtBearer(
                serviceName: keycloakServiceName,
                realm: settings.Realm!,
                options =>
                {
                    // In development we often run Keycloak over HTTP; enable HTTPS metadata outside of dev
                    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

                    options.Audience = settings.Audience;
                    // for service to service calls we don't validate the issuer
                    options.TokenValidationParameters.ValidateIssuer = false;
                }
            );

        return builder;
    }
    public static TBuilder AddJwtAuthentication<TBuilder>(
        this TBuilder builder,
        string keycloakServiceName) where TBuilder : IHostApplicationBuilder
    {
        var settings = builder.Configuration
            .GetSection(KeycloakSettingsOptions.SectionName)
            .Get<KeycloakSettingsOptions>();

        Guard.Against.Null(settings, nameof(settings));
        builder.Services
            .AddServiceDiscovery()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddKeycloakJwtBearer(
                serviceName: keycloakServiceName,
                realm: settings.Realm!,
                options =>
                {
                    // In development we often run Keycloak over HTTP; enable HTTPS metadata outside of dev
                    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

                    options.Audience = settings.Audience;
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters.NameClaimType = "preferred_username";
                    options.TokenValidationParameters.RoleClaimType = "roles";
                }
            );

        return builder;
    }

    public static TBuilder AddCookieOidcAuthentication<TBuilder>(
        this TBuilder builder,
        string keycloakServiceName) where TBuilder : IHostApplicationBuilder
    {
        var settings = builder.Configuration
            .GetSection(KeycloakSettingsOptions.SectionName)
            .Get<KeycloakSettingsOptions>();

        Guard.Against.Null(settings, nameof(settings));

        builder.Services
            .AddServiceDiscovery()
            .AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddKeycloakOpenIdConnect(
                serviceName: keycloakServiceName,
                realm: settings.Realm!,
                options =>
                {
                    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

                    options.ClientId = settings.ClientId;
                    options.ClientSecret = settings.ClientSecret;

                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.ResponseType = OpenIdConnectResponseType.Code;

                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.MapInboundClaims = false;
                    options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
                    options.TokenValidationParameters.RoleClaimType = "roles";
                }
            );

        // Configure automatic cookie refresh using shared helper.
        builder.Services.ConfigureCookieOidc(
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme,
            lg => lg.GetPathByName(AuthRouteNames.CookieOidcLogout)!
        );

        return builder;
    }
    private static IServiceCollection ConfigureCookieOidc(this IServiceCollection services,
        string cookieScheme,
        string oidcScheme,
        Func<LinkGenerator, string> logoutPathFactory
    )
    {
        services.AddSingleton<CookieOidcRefresher>();
        services.AddOptions<CookieAuthenticationOptions>(cookieScheme)
            .Configure<CookieOidcRefresher>((cookieOptions, refresher) =>
            {
                cookieOptions.Events.OnValidatePrincipal = context
                    => refresher.ValidateOrRefreshCookieAsync(context, oidcScheme, logoutPathFactory);
            });

        return services;
    }
}
