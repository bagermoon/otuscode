using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestoRate.BuildingBlocks.Diagnostics;

namespace RestoRate.BuildingBlocks.Data.Migrations;

/// <summary>
/// Static entrypoint that resolves and executes a non-static MigrationExecutor for a given DbContext.
/// Centralizes the ActivitySource and keeps call sites simple.
/// </summary>
public static class DbMigrationRunner
{
    private static readonly ActivitySource ActivitySource = new(ActivitySources.DbMigrations);

    public static async Task RunAsync<TContext>(IServiceProvider services, CancellationToken ct = default)
        where TContext : DbContext
    {
        await using var scope = services.CreateAsyncScope();
        var provider = scope.ServiceProvider;
        var executor = new MigrationExecutor<TContext>(provider, ActivitySource);
        await executor.MigrateAsync(ct);
    }
}
