using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using RestoRate.Abstractions.Identity;

namespace RestoRate.Auth.Identity;

public static class IdentityExtensions
{
    public static TBuilder AddIdentityServices<TBuilder>(
        this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IUserContextProvider, HttpContextUserContextProvider>();
        builder.Services.AddUserContext();
        return builder;
    }
}
