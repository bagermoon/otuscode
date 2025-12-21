using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using RestoRate.Auth.Authentication;

namespace Microsoft.AspNetCore.Routing;

public static class AuthRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapCookieOidcAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("");

        group.MapGet("/login", (string? returnUrl) =>
            TypedResults.Challenge(GetAuthProperties(returnUrl)))
                .AllowAnonymous()
                .WithName(AuthRouteNames.CookieOidcLogin);

        group.MapPost("/logout", ([FromForm] string? returnUrl) =>
            TypedResults.SignOut(GetAuthProperties(returnUrl),
                [
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    OpenIdConnectDefaults.AuthenticationScheme,
                ]
            )
        )
        .WithName(AuthRouteNames.CookieOidcLogout);

        return group;
    }

    private static AuthenticationProperties GetAuthProperties(string? returnUrl)
    {
        const string pathBase = "/";

        if (string.IsNullOrEmpty(returnUrl))
        {
            returnUrl = pathBase;
        }
        else if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
        {
            returnUrl = new Uri(returnUrl, UriKind.Absolute).PathAndQuery;
        }
        else if (returnUrl[0] != '/')
        {
            returnUrl = $"{pathBase}{returnUrl}";
        }

        return new AuthenticationProperties { RedirectUri = returnUrl };
    }
}
