using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;

using RestoRate.IntegrationTests;
using RestoRate.ServiceDefaults;

namespace RestoRate.Restaurant.IntegrationTests.Tests;

[Collection("AspireAppHost collection")]
public class IntegrationTest1(
    AspireAppHost host
) : BasePageTest
{
    [Fact]
    public async Task DashboardIsLoaded()
    {
        var dashboardUrl = host.GetEndpoint(AppHostProjects.BlazorDashboard).ToString();

        await Page.GotoAsync(dashboardUrl);
        // Assert: Logout button is visible by form action URL
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Logout" })).ToBeVisibleAsync();
    }
    public override BrowserNewContextOptions ContextOptions()
    {
        var options = base.ContextOptions();
        options.StorageStatePath = PlaywrightAuthHelper.GetAuthStatePath("admin");
    
        return options;
    }
}
