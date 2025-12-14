using Microsoft.AspNetCore.Http;

using RestoRate.Abstractions.Identity;

namespace RestoRate.Auth.Identity;

public sealed class HttpContextUserContextProvider(IHttpContextAccessor httpContextAccessor) : IUserContextProvider
{
    public int Priority => 0;

    public bool TryGet(out IUserContext userContext)
    {
        if (httpContextAccessor.HttpContext is null)
        {
            userContext = default!;
            return false;
        }

        if (!HttpContextUserContext.TryGetUserContext(httpContextAccessor, out var httpContextUserContext))
        {
            userContext = default!;
            return false;
        }

        userContext = httpContextUserContext;
        return true;
    }
}
