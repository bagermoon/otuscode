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
        services.AddTransient<IDbSeeder<TContext>, TDbSeeder>();
        services.ConfigureDbContext<TContext>((sp, options) =>
        {
            options
                .UseSeeding((context, _) =>
                {
                    var seeder = sp.GetRequiredService<IDbSeeder<TContext>>();
                    seeder.SeedAsync((TContext)context).GetAwaiter().GetResult();
                })
                .UseAsyncSeeding(async (context, _, token) =>
                {
                    var seeder = sp.GetRequiredService<IDbSeeder<TContext>>();
                    await seeder.SeedAsync((TContext)context);
                });
        });
        return services.AddMigration<TContext>();
    }
}
