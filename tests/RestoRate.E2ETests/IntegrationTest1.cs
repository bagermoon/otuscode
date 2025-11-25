using RestoRate.E2ETests.Auth;
using RestoRate.E2ETests.Base;

namespace RestoRate.E2ETests;

[User(TestUser.Admin)]
public class IntegrationTest1(AspireAppHost appHost) : BasePageTest(appHost)
{
    [Fact]
    public async Task DashboardIsLoaded()
    {
        await Page.GotoAsync("/");
        // Assert: Logout button is visible by form action URL
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Logout" })).ToBeVisibleAsync();
    }
}
