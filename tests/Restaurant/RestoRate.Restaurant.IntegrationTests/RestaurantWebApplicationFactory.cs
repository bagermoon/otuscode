using MassTransit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using RestoRate.BuildingBlocks.Data.Migrations;
using RestoRate.Restaurant.Infrastructure.Data;
using RestoRate.ServiceDefaults;
using Testcontainers.PostgreSql;

namespace RestoRate.Restaurant.IntegrationTests;

public class RestaurantWebApplicationFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
            .WithDatabase("resto_rate_integration_tests")
            .WithUsername("resto_rate")
            .WithPassword("resto_rate_password")
            .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Add MassTransit In-Memory Test Harness
            services.AddMassTransitTestHarness(cfg =>
            {
                // Register your consumers here
                // You can configure sagas, activities, etc. as needed
            });
        });
    }

    public async ValueTask InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        // Set environment variable before host is built
        Environment.SetEnvironmentVariable($"ConnectionStrings__{AppHostProjects.RestaurantDb}", _postgreSqlContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("Aspire__Npgsql__EntityFrameworkCore__PostgreSQL__DisableHealthChecks", "true");
        Environment.SetEnvironmentVariable("Aspire__Npgsql__EntityFrameworkCore__PostgreSQL__DisableTracing", "true");
        Environment.SetEnvironmentVariable("Aspire__Npgsql__EntityFrameworkCore__PostgreSQL__DisableMetrics", "true");
        // Apply EF Core migrations
        await DbMigrationRunner.RunAsync<RestaurantDbContext>(Services);
    }

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
/// <summary>
/// <see cref="https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync"/>
/// </summary>
    public override async ValueTask DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
        await base.DisposeAsync();
    }
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
}
