namespace RestoRate.Testing.Common;

public static class ContainerEnvironmentHelpers
{
    public static void SetPostgresEnvironmentVariables(string connectionString, string connectionName)
    {
        Environment.SetEnvironmentVariable($"ConnectionStrings__{connectionName}", connectionString);
        Environment.SetEnvironmentVariable("Aspire__Npgsql__EntityFrameworkCore__PostgreSQL__DisableHealthChecks", "true");
        Environment.SetEnvironmentVariable("Aspire__Npgsql__EntityFrameworkCore__PostgreSQL__DisableTracing", "true");
        Environment.SetEnvironmentVariable("Aspire__Npgsql__EntityFrameworkCore__PostgreSQL__DisableMetrics", "true");
    }

    public static void SetMongoEnvironmentVariables(string connectionString, string connectionName)
    {
        Environment.SetEnvironmentVariable($"ConnectionStrings__{connectionName}", connectionString);
        Environment.SetEnvironmentVariable("Aspire__MongoDB__Driver__DisableHealthChecks", "true");
        Environment.SetEnvironmentVariable("Aspire__MongoDB__Driver__DisableTracing", "true");
    }
}
