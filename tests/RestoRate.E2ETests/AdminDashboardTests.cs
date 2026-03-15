namespace RestoRate.E2ETests;

[User(TestUser.Admin)]
public class AdminDashboardTests(AspireAppHost appHost) : BasePageTest(appHost)
{
    [Fact]
    public async Task AuthorizedUser_SeesUsersBadge()
    {
        await Page.GotoAsync("/");
        // Assert: 
        await Page.GetByRole(AriaRole.Toolbar).Locator("a[href='user-claims']").IsVisibleAsync();

    }
}
