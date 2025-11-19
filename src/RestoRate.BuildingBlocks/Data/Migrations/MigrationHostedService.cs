using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RestoRate.BuildingBlocks.Data.Migrations;

internal sealed class MigrationHostedService<TContext>(
        IServiceProvider serviceProvider,
        IHostApplicationLifetime hostApplicationLifetime,
        ILogger<MigrationHostedService<TContext>> logger)
        : BackgroundService where TContext : DbContext
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            logger.MigrationStarting(typeof(TContext).Name);
            await DbMigrationRunner.RunAsync<TContext>(serviceProvider, stoppingToken);
            logger.MigrationCompleted(typeof(TContext).Name);
        }
        catch (Exception ex)
        {
            logger.MigrationFailed(typeof(TContext).Name, ex);
            throw;
        }
        finally
        {
            hostApplicationLifetime.StopApplication();
        }
    }
}
