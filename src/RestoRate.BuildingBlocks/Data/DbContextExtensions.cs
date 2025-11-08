using System;
using System.Diagnostics.CodeAnalysis;

using Aspire.Npgsql.EntityFrameworkCore.PostgreSQL;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using RestoRate.BuildingBlocks.Data.Interceptors;

namespace RestoRate.BuildingBlocks.Data;

public static class DbContextExtensions
{
    private const DynamicallyAccessedMemberTypes RequiredByEF = DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties;
    public static IHostApplicationBuilder AddPostgresDbContext<[DynamicallyAccessedMembers(RequiredByEF)] TContext>(
        this IHostApplicationBuilder builder,
        string connectionName,
        Action<NpgsqlEntityFrameworkCorePostgreSQLSettings>? configureSettings = null,
        Action<IServiceProvider, DbContextOptionsBuilder>? configureDbContextOptions = null
    )
    where TContext : DbContext
    {
        builder.Services.AddDbContextPool<TContext>((sp, options) =>
        {
            options
                .UseNpgsql(builder.Configuration.GetConnectionString(connectionName))
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetRequiredService<EventDispatchInterceptor>());

            configureDbContextOptions?.Invoke(sp, options);
        });

        builder.Services.TryAddScoped<EventDispatchInterceptor>();
        builder.EnrichNpgsqlDbContext<TContext>(configureSettings);

        return builder;
    }
}
