using Microsoft.AspNetCore.Authorization;

namespace RestoRate.Auth.Authorization;

public static class AuthorizationExtensions
{
    public static AuthorizationBuilder AddDefaultAuthenticationPolicy(this AuthorizationBuilder builder)
    {
        return builder.SetDefaultPolicy(new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build()
        );
    }
    public static AuthorizationBuilder AddAdminPolicies(this AuthorizationBuilder builder)
    {
        return builder.AddPolicy(PolicyNames.RequireAdminRole, policy =>
            policy.RequireRole("admin"));
    }
}
