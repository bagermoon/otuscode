using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RestoRate.Testing.Common.Auth;

public class TestAuthHandler : AuthenticationHandler<TestAuthOptions>
{
    public TestAuthHandler(IOptionsMonitor<TestAuthOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {}
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var user = TestUsers.Get(Options.User);
    
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, user.Name) };
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        var result = AuthenticateResult.Success(ticket);

        return Task.FromResult(result);
    }
}

public class TestAuthOptions : AuthenticationSchemeOptions
{
    public TestUser User { get; set; } = TestUser.Anonymous;
}