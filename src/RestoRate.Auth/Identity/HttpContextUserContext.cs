using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Http;

using RestoRate.Abstractions.Identity;

namespace RestoRate.Auth.Identity;

public class HttpContextUserContext(IHttpContextAccessor http) : IUserContext
{
    private readonly ClaimsPrincipal _user = http.HttpContext?.User ?? new ClaimsPrincipal();
    public Guid UserId => Guid.TryParse(_user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var guid)
        ? guid
        : Guid.Empty;

    public bool IsAuthenticated => _user.Identity?.IsAuthenticated ?? false;
    public string Name => _user.Identity?.Name ?? string.Empty;

    public string FullName =>
        _user.FindFirst(JwtRegisteredClaimNames.Name)?.Value
        ?? string.Empty;

    public string Email =>
        _user.FindFirst(JwtRegisteredClaimNames.Email)?.Value
        ?? string.Empty;

    public IReadOnlyCollection<string> Roles =>
        _user.FindAll("roles").Select(c => c.Value).ToArray();
}