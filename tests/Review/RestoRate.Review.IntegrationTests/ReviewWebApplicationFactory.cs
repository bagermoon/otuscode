using MassTransit;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

using MongoDB.Driver;
using RestoRate.ServiceDefaults;
using Testcontainers.MongoDb;

namespace RestoRate.Review.IntegrationTests;

public class ReviewWebApplicationFactory
    : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MongoDbContainer _mongoDbContainer = new MongoDbBuilder().Build();
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
    public async ValueTask InitializeAsync()
    {
        await _mongoDbContainer.StartAsync();

        var connectionString = _mongoDbContainer.GetConnectionString();
        var connectionStringBuilder = new MongoUrlBuilder(connectionString)
        {
            DatabaseName = "reviewdb",
            AuthenticationSource = "admin",
        };

        // Set environment variable before host is built
        Environment.SetEnvironmentVariable($"ConnectionStrings__{AppHostProjects.ReviewDb}", connectionStringBuilder.ToString());
        Environment.SetEnvironmentVariable("Aspire__MongoDB__Driver__DisableHealthChecks", "true");
        Environment.SetEnvironmentVariable("Aspire__MongoDB__Driver__DisableTracing", "true");
    }

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    /// <summary>
    /// <see cref="https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-disposeasync"/>
    /// </summary>
    public override async ValueTask DisposeAsync()
    {
        await _mongoDbContainer.DisposeAsync();
        await base.DisposeAsync();
    }
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
}
