using System.Data.Common;

using MassTransit;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;

using RestoRate.BuildingBlocks.Data.Interceptors;
using RestoRate.BuildingBlocks.Data.Migrations;
using RestoRate.Restaurant.Infrastructure.Data;
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
            var dbContextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == 
                        typeof(DbContextOptions<RestaurantDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            var dbConnectionDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbConnection));

            if (dbConnectionDescriptor != null)
            {
                services.Remove(dbConnectionDescriptor);
            }

            services.AddDbContext<RestaurantDbContext>((sp, options) =>
            {
                options
                    .UseNpgsql(_postgreSqlContainer.GetConnectionString())
                    .AddInterceptors(sp.GetRequiredService<EventDispatchInterceptor>())
                ;
            }, ServiceLifetime.Singleton);

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
