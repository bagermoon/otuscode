using Microsoft.Extensions.Logging;

namespace RestoRate.BuildingBlocks.Diagnostics;

public static class LoggingEventIds
{
    // Reserve ranges per domain: 1000-1099 Migrations, 1100-1199 Messaging, etc.
    public static readonly EventId DbMigrationStart = new(1000, nameof(DbMigrationStart));
    public static readonly EventId DbMigrationError = new(1001, nameof(DbMigrationError));
    public static readonly EventId DbMigrationCompleted = new(1002, nameof(DbMigrationCompleted));
}
