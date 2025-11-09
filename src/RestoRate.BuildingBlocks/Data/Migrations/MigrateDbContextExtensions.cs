using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestoRate.BuildingBlocks.Diagnostics;

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
        services.AddScoped<IDbSeeder<TContext>, TDbSeeder>();
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

    private class MigrationHostedService<TContext>(
        IServiceProvider serviceProvider,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<MigrationHostedService<TContext>> logger)
        : BackgroundService where TContext : DbContext
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Log.MigrationStarting(logger, typeof(TContext).Name);
                await DbMigrationRunner.RunAsync<TContext>(serviceProvider, stoppingToken);
                Log.MigrationCompleted(logger, typeof(TContext).Name);
            }
            catch (Exception ex)
            {
                Log.MigrationFailed(logger, typeof(TContext).Name, ex);
                throw;
            }
            finally
            {
                hostApplicationLifetime.StopApplication();
            }
        }
    }

    private static partial class Log
    {
        private static readonly Action<ILogger, string, Exception?> _migrationStarting =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                LoggingEventIds.DbMigrationStart,
                "Starting database migration for {DbContext}");

        private static readonly Action<ILogger, string, Exception?> _migrationCompleted =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                LoggingEventIds.DbMigrationCompleted,
                "Database migration completed for {DbContext}");

        private static readonly Action<ILogger, string, Exception?> _migrationFailed =
            LoggerMessage.Define<string>(
                LogLevel.Error,
                LoggingEventIds.DbMigrationError,
                "Database migration failed for {DbContext}");

        public static void MigrationStarting(ILogger logger, string dbContext)
            => _migrationStarting(logger, dbContext, null);

        public static void MigrationCompleted(ILogger logger, string dbContext)
            => _migrationCompleted(logger, dbContext, null);

        public static void MigrationFailed(ILogger logger, string dbContext, Exception ex)
            => _migrationFailed(logger, dbContext, ex);
    }
}
