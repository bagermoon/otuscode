using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RestoRate.Abstractions.Identity;

public static class UserContextServiceCollectionExtensions
{
    /// <summary>
    /// Registers a scoped IUserContext that is resolved using registered IUserContextProvider implementations.
    /// Safe to call multiple times; the first registration wins.
    /// </summary>
    public static IServiceCollection AddUserContext(this IServiceCollection services)
    {
        services.TryAddScoped<IUserContext>(sp =>
        {
            var providers = sp.GetServices<IUserContextProvider>()
                .OrderByDescending(p => p.Priority);

            foreach (var provider in providers)
            {
                if (provider.TryGet(out var ctx))
                    return ctx;
            }

            return AnonymousUserContext.Instance;
        });

        return services;
    }
}
