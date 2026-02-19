using RestoRate.ServiceDefaults;
using RestoRate.Auth.Authentication;
using RestoRate.Auth.Authorization;
using RestoRate.Auth.Identity;

namespace RestoRate.RatingService.Api.Configurations;

internal static class AuthConfigs
{
    public static IHostApplicationBuilder ConfigureAuthentication(this IHostApplicationBuilder builder)
    {
        builder.AddJwtAuthentication(AppHostProjects.Keycloak);

        builder.Services.AddAuthorizationBuilder()
            .AddDefaultAuthenticationPolicy()
            .AddAdminPolicies();

        builder.AddIdentityServices();

        return builder;
    }
}
