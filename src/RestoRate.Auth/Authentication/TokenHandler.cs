using System.Net.Http.Headers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using RestoRate.Abstractions.Identity;
using RestoRate.Auth.Authentication.ClientCredentials;

namespace RestoRate.Auth.Authentication;

public sealed class TokenHandler(
    IHttpContextAccessor httpContextAccessor,
    IClientCredentialsTokenProvider clientCredentialsTokenProvider) : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization is not null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var httpContext = _httpContextAccessor.HttpContext;

        string? accessToken = null;
        var userContext = httpContext?.RequestServices.GetRequiredService<IUserContext>();
        if (httpContext is not null && userContext?.IsAuthenticated == true)
        {
            accessToken = await httpContext.GetTokenAsync("access_token");
        }

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            accessToken = await clientCredentialsTokenProvider.GetAccessTokenAsync(cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
