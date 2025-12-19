namespace RestoRate.Testing.Common;

public static class ContainerConfigurationHelpers
{
    public static IEnumerable<KeyValuePair<string, string?>> GetPostgresConfiguration(string connectionString, string connectionName)
        => new Dictionary<string, string?>
        {
            ["Aspire:Npgsql:EntityFrameworkCore:PostgreSQL:DisableHealthChecks"] = true.ToString(),
            ["Aspire:Npgsql:EntityFrameworkCore:PostgreSQL:DisableTracing"] = true.ToString(),
            ["Aspire:Npgsql:EntityFrameworkCore:PostgreSQL:DisableMetrics"] = true.ToString(),
            [$"ConnectionStrings:{connectionName}"] = connectionString,
        };

    public static IEnumerable<KeyValuePair<string, string?>> GetMongoConfiguration(string connectionString, string connectionName)
        => new Dictionary<string, string?>
        {
            ["Aspire:MongoDB:Driver:DisableHealthChecks"] = true.ToString(),
            ["Aspire:MongoDB:Driver:DisableTracing"] = true.ToString(),
            [$"ConnectionStrings:{connectionName}"] = connectionString,
        };
}
