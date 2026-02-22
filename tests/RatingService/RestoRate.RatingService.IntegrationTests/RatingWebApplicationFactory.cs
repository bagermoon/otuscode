using MassTransit;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using RestoRate.ServiceDefaults;
using Testcontainers.MongoDb;
using DotNet.Testcontainers.Containers;
using RestoRate.BuildingBlocks.Messaging;
using Testcontainers.Redis;

namespace RestoRate.RatingService.IntegrationTests;

public class RatingWebApplicationFactory
    : BaseWebApplicationFactory<Program>
{
    private readonly MongoDbContainer _mongo = new MongoDbBuilder("docker.io/library/mongo:8.2").Build();
    private readonly RedisContainer _redis = new RedisBuilder("docker.io/library/redis:8.4-alpine").Build();
    protected override IReadOnlyList<IContainer> Containers => [_mongo, _redis];

    protected override Task<IHost> CreateHostAsync(IHostBuilder builder)
    {
        var connectionString = _mongo.GetConnectionString();
        var connectionStringBuilder = new MongoUrlBuilder(connectionString)
        {
            DatabaseName = AppHostProjects.RatingDb,
            AuthenticationSource = "admin",
        };

        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(
                ContainerConfigurationHelpers.GetMongoConfiguration(
                    connectionString: connectionStringBuilder.ToString(),
                    connectionName: AppHostProjects.RatingDb
                )
            );

            config.AddInMemoryCollection(
                ContainerConfigurationHelpers.GetRedisConfiguration(
                    connectionString: _redis.GetConnectionString(),
                    connectionName: AppHostProjects.RatingDb
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
        builder.ConfigureLogging(logging =>
        {
            // Avoid Windows EventLog logger issues during WebApplicationFactory disposal.
            logging.ClearProviders();
        });

        builder.AddMassTransitInMemoryTestHarness();
    }
}
