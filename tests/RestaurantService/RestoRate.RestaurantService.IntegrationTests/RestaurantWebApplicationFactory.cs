using DotNet.Testcontainers.Containers;

using MassTransit;

using Microsoft.AspNetCore.TestHost;

using RestoRate.BuildingBlocks.Data.Migrations;
using RestoRate.BuildingBlocks.Messaging;
using RestoRate.RestaurantService.Infrastructure.Data;
using RestoRate.ServiceDefaults;

using Testcontainers.PostgreSql;

namespace RestoRate.RestaurantService.IntegrationTests;

public class RestaurantWebApplicationFactory
    : BaseWebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
            .WithDatabase(AppHostProjects.RestaurantDb)
            .Build();
    protected override IReadOnlyList<IContainer> Containers => [_postgres];

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.AddMassTransitInMemoryTestHarness();
    }

    protected override async Task<IHost> CreateHostAsync(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(
                ContainerConfigurationHelpers.GetPostgresConfiguration(
                    connectionString: _postgres.GetConnectionString(),
                    connectionName: AppHostProjects.RestaurantDb
                )
            );
        });

        var host = await base.CreateHostAsync(builder);
        await DbMigrationRunner.RunAsync<RestaurantDbContext>(host.Services);

        return host;
    }
}
