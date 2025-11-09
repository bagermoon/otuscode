using System;

using RestoRate.Common;
using RestoRate.ServiceDefaults;

namespace RestoRate.Restaurant.Api.Configurations;

internal static class AuthConfigs
{
    public static IHostApplicationBuilder ConfigureAuthentication(this IHostApplicationBuilder builder)
    {
        builder.AddKeycloakJwtAuthentication(AppHostProjects.Keycloak);

        builder.Services.AddAuthorizationBuilder()
            .AddPolicy("AdminGroup", policy =>
                policy.RequireRole("admin")); // Checks for a "roles" claim with value "admin"

        return builder;
    }
}
