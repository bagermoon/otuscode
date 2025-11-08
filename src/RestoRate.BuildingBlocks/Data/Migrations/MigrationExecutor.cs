using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RestoRate.BuildingBlocks.Data.Migrations;

internal sealed class MigrationExecutor<TContext> where TContext : DbContext
{
    private readonly IServiceProvider _root;
    private readonly ActivitySource _activitySource;

    public MigrationExecutor(
        IServiceProvider root,
        ActivitySource activitySource)
    {
        _root = root;
        _activitySource = activitySource;
    }

    public async Task MigrateAsync(CancellationToken ct = default)
    {
        await using var scope = _root.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        using var activity = _activitySource.StartActivity($"Migration operation {typeof(TContext).Name}");

        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(() => InvokeMigrationAsync(context, ct));
    }

    private async Task InvokeMigrationAsync(TContext context, CancellationToken ct)
    {
        using var activity = _activitySource.StartActivity($"Migrating {typeof(TContext).Name}");

        try
        {
            await context.Database.MigrateAsync(ct);
            await NpgsqlTypeReloader.ReloadTypesIfNeededAsync(context, ct);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }
    }

    // No logging here to avoid duplication; tracing only via ActivitySource.
}
