using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Http;

using RestoRate.Abstractions.Identity;

namespace RestoRate.Auth.Identity;

public sealed class HttpContextUserContext : IUserContext
{
    private HttpContextUserContext()
    { }
    public Guid UserId { get; init; }
    public bool IsAuthenticated { get; init; }
    public string Name { get; init; } = default!;
    public string FullName { get; init; } = default!;
    public string Email { get; init; } = default!;

    public IReadOnlyCollection<string> Roles { get; init; } = [];

    public static bool TryGetUserContext(IHttpContextAccessor http, out HttpContextUserContext userContext)
    {
        userContext = default!;
        var user = http.HttpContext?.User ?? new ClaimsPrincipal();

        if (HttpContextUserReader.TryGetUserId(user, out var userId))
        {
            userContext = new HttpContextUserContext
            {
                UserId = userId,
                IsAuthenticated = HttpContextUserReader.IsAuthenticated(user),
                Name = HttpContextUserReader.GetUserName(user),
                FullName = HttpContextUserReader.GetUserFullName(user),
                Email = HttpContextUserReader.GetUserEmail(user),
                Roles = HttpContextUserReader.GetUserRoles(user)
            };
            return true;
        }
        return false;
    }
}