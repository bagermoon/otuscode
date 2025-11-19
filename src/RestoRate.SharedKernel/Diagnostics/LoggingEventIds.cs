namespace RestoRate.SharedKernel.Diagnostics;

public static partial class LoggingEventIds
{
    // 1000-1099: Migrations
    public const int DbMigrationStart = 1000;
    public const int DbMigrationError = 1001;
    public const int DbMigrationCompleted = 1002;

    // 1100-1199: Request logging
    public const int HandlingRequestReport = 1100;
    public const int HandlingRequestReportProperty = 1101;
    public const int HandlingRequestReportTime = 1102;
}
