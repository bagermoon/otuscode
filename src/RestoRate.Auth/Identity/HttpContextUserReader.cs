using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RestoRate.Auth.Identity;

public static class HttpContextUserReader
{
    public static bool TryGetUserId(ClaimsPrincipal user, out Guid userId)
        => Guid.TryParse(user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out userId);

    public static string GetUserName(ClaimsPrincipal user) =>
        user.Identity?.Name ?? string.Empty;

    public static string GetUserFullName(ClaimsPrincipal user)
        => user.FindFirst(JwtRegisteredClaimNames.Name)?.Value
        ?? string.Empty;

    public static string GetUserEmail(ClaimsPrincipal user)
        => user.FindFirst(JwtRegisteredClaimNames.Email)?.Value
        ?? string.Empty;

    public static string[] GetUserRoles(ClaimsPrincipal user)
        => user.FindAll("roles").Select(c => c.Value).ToArray();

    public static bool IsAuthenticated(ClaimsPrincipal user)
        => user.Identity?.IsAuthenticated ?? false;
}
