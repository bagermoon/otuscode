namespace RestoRate.E2ETests;

[User(TestUser.Admin)]
public class AdminDashboardTests(AspireAppHost appHost) : BasePageTest(appHost)
{
    [Fact]
    public async Task AuthorizedUser_SeesLogoutButton()
    {
        await Page.GotoAsync("/");
        // Assert: Logout button is visible by form action URL
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Выход" })).ToBeVisibleAsync();
    }
}
