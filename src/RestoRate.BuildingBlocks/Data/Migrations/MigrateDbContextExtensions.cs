using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RestoRate.BuildingBlocks.Data.Migrations;

public static class MigrateDbContextExtensions
{
    public static IServiceCollection AddMigration<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        return services.AddHostedService<MigrationHostedService<TContext>>();
    }

    public static IServiceCollection AddMigration<TContext, TDbSeeder>(this IServiceCollection services)
        where TContext : DbContext
        where TDbSeeder : class, IDbSeeder<TContext>
    {
        // Seeders are registered as transient because they are stateless and resolved only during
        // application startup seeding, and multiple seeders may be registered per DbContext.
        services.AddTransient<IDbSeeder<TContext>, TDbSeeder>();
        services.ConfigureDbContext<TContext>((sp, options) =>
        {
            options
                .UseSeeding((context, _) =>
                {
                    var seeders = sp.GetServices<IDbSeeder<TContext>>();
                    foreach (var seeder in seeders)
                    {
                        seeder.SeedAsync((TContext)context).GetAwaiter().GetResult();
                    }

                })
                .UseAsyncSeeding(async (context, _, token) =>
                {
                    var seeders = sp.GetServices<IDbSeeder<TContext>>();
                    foreach (var seeder in seeders)
                    {
                        await seeder.SeedAsync((TContext)context, token);
                    }
                });
        });
        return services.AddMigration<TContext>();
    }
}
