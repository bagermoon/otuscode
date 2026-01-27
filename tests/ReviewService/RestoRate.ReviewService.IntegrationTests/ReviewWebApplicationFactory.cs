using MassTransit;
using Microsoft.AspNetCore.TestHost;
using MongoDB.Driver;
using RestoRate.ServiceDefaults;
using Testcontainers.MongoDb;
using DotNet.Testcontainers.Containers;
using RestoRate.BuildingBlocks.Messaging;

namespace RestoRate.ReviewService.IntegrationTests;

public class ReviewWebApplicationFactory
    : BaseWebApplicationFactory<Program>
{
    private readonly MongoDbContainer _mongo = new MongoDbBuilder().Build();

    protected override IReadOnlyList<IContainer> Containers => [_mongo];

    protected override Task<IHost> CreateHostAsync(IHostBuilder builder)
    {
        var connectionString = _mongo.GetConnectionString();
        var connectionStringBuilder = new MongoUrlBuilder(connectionString)
        {
            DatabaseName = AppHostProjects.ReviewDb,
            AuthenticationSource = "admin",
        };

        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(
                ContainerConfigurationHelpers.GetMongoConfiguration(
                    connectionString: connectionStringBuilder.ToString(),
                    connectionName: AppHostProjects.ReviewDb
                )
            );

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MassTransit:UseMongoDbSagaOutbox"] = "false",
            });
        });

        return base.CreateHostAsync(builder);
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.AddMassTransitInMemoryTestHarness();
    }
}
