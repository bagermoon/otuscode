using Microsoft.Extensions.Logging;

using RestoRate.SharedKernel.Diagnostics;

namespace RestoRate.BuildingBlocks.Data.Migrations;

internal static partial class MigrationLogger
{
    [LoggerMessage(
        EventId = LoggingEventIds.DbMigrationStart, Level = LogLevel.Information,
        Message = "Starting database migration for {DbContext}")]
    public static partial void MigrationStarting(this ILogger logger, string dbContext);

    [LoggerMessage(
            EventId = LoggingEventIds.DbMigrationCompleted, Level = LogLevel.Information,
            Message = "Database migration completed for {DbContext}")]
    public static partial void MigrationCompleted(this ILogger logger, string dbContext);

    [LoggerMessage(
            EventId = LoggingEventIds.DbMigrationError, Level = LogLevel.Error,
            Message = "Database migration failed for {DbContext}")]
    public static partial void MigrationFailed(this ILogger logger, string dbContext, Exception ex);
}
