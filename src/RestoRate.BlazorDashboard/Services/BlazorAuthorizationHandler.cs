using System.Net.Http.Headers;

namespace RestoRate.BlazorDashboard.Services;

public class BlazorAuthorizationHandler(BlazorTokenProvider tokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(tokenProvider.AccessToken) && request.Headers.Authorization == null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenProvider.AccessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
