using System.Collections.Concurrent;

using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace RestoRate.BuildingBlocks.Data.Migrations;

/// <summary>
/// Reloads PostgreSQL types for an EF Core DbContext using Npgsql.
/// Ensures the operation runs once per process per connection string to avoid repeated catalog queries.
/// See: https://github.com/npgsql/efcore.pg/issues/292#issuecomment-388608426
/// </summary>
internal static class NpgsqlTypeReloader
{
    private static readonly ConcurrentDictionary<string, bool> Reloaded = new();

    public static async Task ReloadTypesIfNeededAsync(DbContext context, CancellationToken ct = default)
    {
        if (context.Database.GetDbConnection() is not NpgsqlConnection conn) return;

        var key = conn.ConnectionString;
        if (Reloaded.ContainsKey(key)) return;

        var shouldClose = conn.State != System.Data.ConnectionState.Open;
        if (shouldClose) await conn.OpenAsync(ct);
        await conn.ReloadTypesAsync(ct);
        if (shouldClose) await conn.CloseAsync();

        Reloaded[key] = true;
    }
}
