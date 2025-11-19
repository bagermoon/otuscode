using System.Diagnostics.CodeAnalysis;

using Ardalis.GuardClauses;
using Ardalis.SharedKernel;

using Aspire.Npgsql.EntityFrameworkCore.PostgreSQL;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using MongoDB.EntityFrameworkCore.Extensions;
using MongoDB.EntityFrameworkCore.Infrastructure;

using RestoRate.BuildingBlocks.Data.Interceptors;
using RestoRate.SharedKernel;

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
                .AddInterceptors(sp.GetRequiredService<EventDispatchInterceptor>())
            ;

            configureDbContextOptions?.Invoke(sp, options);
        });

        builder.Services.TryAddSingleton<EventDispatchInterceptor>();
        builder.Services.TryAddScoped<IDomainEventDispatcher, NoOpDomainEventDispatcher>();
        builder.EnrichNpgsqlDbContext<TContext>(configureSettings);

        return builder;
    }

    public static IHostApplicationBuilder AddMongoDbContext<[DynamicallyAccessedMembers(RequiredByEF)] TContext>(
        this IHostApplicationBuilder builder,
        string connectionName,
        string? databaseName = null,
        Action<MongoClientSettings>? configureSettings = null,
        Action<IServiceProvider, MongoDbContextOptionsBuilder>? configureDbContextOptions = null
    ) where TContext : DbContext
    {
        builder.AddMongoDBClient(connectionName);
        builder.Services.AddDbContext<TContext>((sp, options) =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var configuration = sp.GetRequiredService<IConfiguration>();

            if (databaseName is null)
            {
                // Get the database name from the client's settings
                var connectionString = configuration.GetConnectionString(connectionName);
                var mongoUrl = new MongoUrl(connectionString);
                databaseName = mongoUrl.DatabaseName;

                Guard.Against.NullOrWhiteSpace(databaseName, nameof(databaseName));
            }
            options.UseMongoDB(
                mongoClient: client,
                databaseName: databaseName,
                builder => configureDbContextOptions?.Invoke(sp, builder)
            );
        });

        builder.Services.TryAddSingleton<EventDispatchInterceptor>();
        builder.Services.TryAddScoped<IDomainEventDispatcher, NoOpDomainEventDispatcher>();

        return builder;
    }
}
