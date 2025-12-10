using MassTransit;
using Microsoft.AspNetCore.TestHost;
using MongoDB.Driver;
using RestoRate.ServiceDefaults;
using Testcontainers.MongoDb;
using DotNet.Testcontainers.Containers;

namespace RestoRate.ReviewService.IntegrationTests;

public class ReviewWebApplicationFactory
    : BaseWebApplicationFactory<Program>
{
    private readonly MongoDbContainer _mongo = new MongoDbBuilder().Build();

    protected override IReadOnlyList<IContainer> Containers => [_mongo];
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumers(typeof(Program).Assembly);
            });
        });
    }
    protected async override Task OnInitializeAsync()
    {
        await _mongo.StartAsync();

        var connectionString = _mongo.GetConnectionString();
        var connectionStringBuilder = new MongoUrlBuilder(connectionString)
        {
            DatabaseName = AppHostProjects.ReviewDb,
            AuthenticationSource = "admin",
        };

        // Set environment variable before host is built
        ContainerEnvironmentHelpers.SetMongoEnvironmentVariables(
            connectionString: connectionStringBuilder.ToString(),
            connectionName: AppHostProjects.ReviewDb
        );
    }
}
