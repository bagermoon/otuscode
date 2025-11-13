using RestoRate.ServiceDefaults;

namespace RestoRate.Review.Api.Configurations;

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