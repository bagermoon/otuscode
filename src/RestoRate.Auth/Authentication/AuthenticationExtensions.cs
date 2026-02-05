using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using RestoRate.Auth.Authentication.ClientCredentials;

namespace RestoRate.Auth.Authentication;

public static class AuthenticationExtensions
{
    public static TBuilder AddGatewayJwtAuthentication<TBuilder>(
        this TBuilder builder,
        string keycloakServiceName) where TBuilder : IHostApplicationBuilder
    {
        var settings = new KeycloakSettingsOptions();
        builder.Configuration
            .GetSection(KeycloakSettingsOptions.SectionName)
            .Bind(settings);

        builder.Services
            .AddServiceDiscovery()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddKeycloakJwtBearer(
                serviceName: keycloakServiceName,
                realm: settings.Realm,
                options =>
                {
                    // In development we often run Keycloak over HTTP; enable HTTPS metadata outside of dev
                    options.RequireHttpsMetadata = builder.Environment.IsProduction();

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
        var settings = new KeycloakSettingsOptions();
        builder.Configuration
            .GetSection(KeycloakSettingsOptions.SectionName)
            .Bind(settings);

        builder.Services
            .AddServiceDiscovery()
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddKeycloakJwtBearer(
                serviceName: keycloakServiceName,
                realm: settings.Realm,
                options =>
                {
                    // In development we often run Keycloak over HTTP; enable HTTPS metadata outside of dev
                    options.RequireHttpsMetadata = builder.Environment.IsProduction();

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
        var settings = new KeycloakSettingsOptions();
        builder.Configuration
            .GetSection(KeycloakSettingsOptions.SectionName)
            .Bind(settings);

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
                realm: settings.Realm,
                options =>
                {
                    options.RequireHttpsMetadata = builder.Environment.IsProduction();

                    options.ClientId = settings.ClientId;
                    options.ClientSecret = settings.ClientSecret;

                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.ResponseType = OpenIdConnectResponseType.Code;

                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.MapInboundClaims = false;
                    options.TokenValidationParameters.NameClaimType = "preferred_username";
                    options.TokenValidationParameters.RoleClaimType = "roles";

                    // Roles are often present only in access_token (not in id_token/userinfo).
                    // The dashboard authorizes against the cookie principal, so we hydrate role claims
                    // onto the principal during sign-in.
                    options.Events ??= new OpenIdConnectEvents();
                    var existingTokenValidated = options.Events.OnTokenValidated;
                    options.Events.OnTokenValidated = async context =>
                    {
                        if (context.Principal?.Identity is ClaimsIdentity identity)
                        {
                            var accessToken = context.TokenEndpointResponse?.AccessToken
                                ?? context.ProtocolMessage?.AccessToken
                                ?? context.Properties?.GetTokenValue("access_token");

                            KeycloakRoleExtractor.AddRolesFromAccessToken(
                                identity: identity,
                                jwtAccessToken: accessToken,
                                clientId: settings.ClientId);
                        }

                        if (existingTokenValidated is not null)
                            await existingTokenValidated(context);
                    };
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
        services.AddMemoryCache();
        services.AddSingleton<CachedTokenManager>();

        services.AddHttpClient<IClientCredentialsTokenClient, KeycloakClientCredentialsTokenClient>(
            KeycloakClientCredentialsTokenClient.HttpClientName, client =>
                client.Timeout = TimeSpan.FromSeconds(60)
            );
        services.AddSingleton<IClientCredentialsTokenProvider, ClientCredentialsTokenProvider>();
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
