using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestoRate.Common;

namespace RestoRate.ServiceDefaults;

public static class AuthenticationExtensions
{
    /// <summary>
    /// Adds JWT Bearer authentication backed by Keycloak discovered via Aspire service discovery.
    /// Loads KeycloakSettings from configuration section "KeycloakSettings".
    /// </summary>
    /// <param name="builder">Host builder</param>
    /// <param name="keycloakServiceName">The Aspire service name for Keycloak (e.g., AppHostProjects.Keycloak)</param>
    public static TBuilder AddKeycloakJwtAuthentication<TBuilder>(
        this TBuilder builder,
        string keycloakServiceName) where TBuilder : IHostApplicationBuilder
    {
        var settings = new KeycloakSettingsOptions();
        builder.Configuration.GetSection(KeycloakSettingsOptions.SectionName).Bind(settings);

        builder.Services
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
                    options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
                    options.TokenValidationParameters.RoleClaimType = "roles";
                }
            );

        return builder;
    }
}
