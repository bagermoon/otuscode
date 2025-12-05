using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Http;

using RestoRate.Abstractions.Identity;

namespace RestoRate.Auth.Identity;

public class HttpContextUserContext : IUserContext
{
    private readonly IHttpContextAccessor _http;

    public HttpContextUserContext(IHttpContextAccessor http) => _http = http;

    public string? UserId =>
        _http.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? _http.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

    public Guid? UserGuid => Guid.TryParse(UserId, out var g) ? g : null;

    public bool IsAuthenticated => _http.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
