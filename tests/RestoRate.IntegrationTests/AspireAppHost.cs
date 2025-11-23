using Aspire.Hosting;

using Microsoft.Extensions.Hosting;

using RestoRate.IntegrationTests;
using RestoRate.ServiceDefaults;

namespace RestoRate.Restaurant.IntegrationTests;

public class AspireAppHost() : DistributedApplicationFactory(typeof(Projects.RestoRate_AppHost)), IAsyncLifetime
{
    public async ValueTask InitializeAsync()
    {
        
        await StartAsync();
        var dashboardUrl = GetEndpoint(AppHostProjects.BlazorDashboard, "https").ToString();
        await Task.WhenAll(
            PlaywrightAuthHelper.SaveAuthStateAsync(dashboardUrl, "admin", "admin"),
            PlaywrightAuthHelper.SaveAuthStateAsync(dashboardUrl, "user", "user")
        );
    }

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
