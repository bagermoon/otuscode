using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Primitives;

namespace RestoRate.Gateway.OutputCaching;

/// <summary>
/// Пользовательская политика кэширования ответов для аутентифицированного эндпоинта списка ресторанов.
/// Политика обеспечивает кэширование только аутентифицированных запросов к эндпоинту /api/restaurants
/// и варьирование записей кэша по соответствующим параметрам запроса.
/// </summary>
internal sealed class RestaurantsAuthenticatedOutputCachePolicy : IOutputCachePolicy
{
    private static readonly string[] VaryByQueryKeys =
    [
        "pageNumber",
        "pageSize",
        "searchTerm",
        "cuisineType",
        "tag",
        "sortBy"
    ];

    private readonly TimeSpan _expiration;

    public RestaurantsAuthenticatedOutputCachePolicy(int expirationMinutes = 5)
    {
        _expiration = TimeSpan.FromMinutes(expirationMinutes);
    }

    ValueTask IOutputCachePolicy.CacheRequestAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;
        var request = httpContext.Request;

        var isEligible =
            (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method)) &&
            httpContext.User.Identity?.IsAuthenticated == true;

        context.EnableOutputCaching = isEligible;
        context.AllowCacheLookup = isEligible;
        context.AllowCacheStorage = isEligible;
        context.AllowLocking = isEligible;

        if (isEligible)
        {
            context.ResponseExpirationTimeSpan = _expiration;
            context.CacheVaryByRules.QueryKeys = VaryByQueryKeys;
        }

        return ValueTask.CompletedTask;
    }

    ValueTask IOutputCachePolicy.ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }

    ValueTask IOutputCachePolicy.ServeResponseAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        if (!context.AllowCacheStorage)
        {
            return ValueTask.CompletedTask;
        }

        var response = context.HttpContext.Response;

        if (!StringValues.IsNullOrEmpty(response.Headers.SetCookie))
        {
            context.AllowCacheStorage = false;
            return ValueTask.CompletedTask;
        }

        if (response.StatusCode != StatusCodes.Status200OK)
        {
            context.AllowCacheStorage = false;
        }

        return ValueTask.CompletedTask;
    }
}
