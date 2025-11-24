using Aspire.Hosting;
using Microsoft.Extensions.Hosting;
using RestoRate.IntegrationTests.Auth;
using RestoRate.ServiceDefaults;

namespace RestoRate.Restaurant.IntegrationTests.Base;

public class AspireAppHost() : DistributedApplicationFactory(typeof(Projects.RestoRate_AppHost)), IAsyncLifetime
{
    public const string EndpointName = "http";
    public async ValueTask InitializeAsync()
    {
        await StartAsync();
        await PlaywrightAuthHelper.SaveAllAuthStatesAsync(DashboardUrl);
    }
    public string DashboardUrl { get => GetEndpoint(AppHostProjects.BlazorDashboard, EndpointName).ToString(); }

    protected override void OnBuilderCreating(
        DistributedApplicationOptions applicationOptions,
        HostApplicationBuilderSettings hostOptions
    )
    {
        applicationOptions.TrustDeveloperCertificate = true;
        applicationOptions.DisableDashboard = false;
    }

    protected override void OnBuilderCreated(
        DistributedApplicationBuilder builder)
    {
        builder.Configuration["ASPIRE_ALLOW_UNSECURED_TRANSPORT"] = "true";
        builder.Configuration["AppHostConfiguration:UseVolumes"] = "false";
        builder.Configuration["AppHostConfiguration:UseDedicatedPorts"] = "false";

        builder.Configuration["AppHostConfiguration:Postgres:LifetimePersistent"] = "false";
        builder.Configuration["AppHostConfiguration:Postgres:UsePgWeb"] = "false";

        builder.Configuration["AppHostConfiguration:Mongo:LifetimePersistent"] = "false";
        builder.Configuration["AppHostConfiguration:Mongo:UseExpress"] = "false";

        builder.Configuration["AppHostConfiguration:Keycloak:LifetimePersistent"] = "false";
        builder.Configuration["AppHostConfiguration:Keycloak:UseKCHostname"] = "false";

        builder.Configuration["AppHostConfiguration:Rabbit:LifetimePersistent"] = "false";
        builder.Configuration["AppHostConfiguration:Rabbit:UseManagementPlugin"] = "false";

        builder.Configuration["AppHostConfiguration:Redis:UseInsight"] = "false";
        builder.Configuration["AppHostConfiguration:Redis:LifetimePersistent"] = "false";

        builder.Configuration["AppHostConfiguration:Scalar:ExplicitStart"] = "true";

        builder.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });
    }
}
