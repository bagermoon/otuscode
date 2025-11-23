using Microsoft.Playwright;

using RestoRate.IntegrationTests.Auth;
using RestoRate.Restaurant.IntegrationTests.Base;
using RestoRate.ServiceDefaults;

namespace RestoRate.Restaurant.IntegrationTests;

[User(TestUser.Admin)]
public class IntegrationTest1(AspireAppHost appHost) : BasePageTest(appHost)
{
    [Fact]
    public async Task DashboardIsLoaded()
    {
        var dashboardUrl = AppHost.GetEndpoint(AppHostProjects.BlazorDashboard).ToString();

        await Page.GotoAsync(dashboardUrl);
        // Assert: Logout button is visible by form action URL
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Logout" })).ToBeVisibleAsync();
    }
}
