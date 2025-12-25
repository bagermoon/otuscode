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

            var ctx = providers
                .Select(p => { return p.TryGet(out var c) ? c : null; })
                .FirstOrDefault(c => c != null);

            return ctx is not null && ctx.IsAuthenticated && !string.IsNullOrEmpty(ctx.Name)
                ? ctx : AnonymousUserContext.Instance;
        });

        return services;
    }
}
