namespace RestoRate.E2ETests.Auth;

public static class PlaywrightAuthHelper
{
    public static async Task SaveAuthStateAsync(string dashboardUrl, TestUser user)
    {
        var username = TestUsers.Get(user).Name;
        var password = TestUsers.Get(user).Password;

        if (password == null)
            return;

        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync(dashboardUrl);
        await page.GetByRole(AriaRole.Toolbar).Locator("a[href='authentication/login']").ClickAsync();
        await page.Locator("input[name='username']").FillAsync(username);
        await page.Locator("input[name='password']").FillAsync(password);
        await page.Locator("button[name='login']").ClickAsync();
        await page.GetByRole(AriaRole.Toolbar).Locator("a[href='user-claims']").IsVisibleAsync();
        

        await context.StorageStateAsync(new() { Path = GetAuthStatePath(user) });
        await browser.CloseAsync();
    }

    public static string GetAuthStatePath(TestUser user)
    {
        var authDir = Path.Combine(AppContext.BaseDirectory, ".auth");
        Directory.CreateDirectory(authDir);
        return Path.Combine(authDir, $"{user}_state.json");
    }

    public static async Task SaveAllAuthStatesAsync(string dashboardUrl)
    {
        await Task.WhenAll(
            Enum.GetValues<TestUser>()
            .Select(user => SaveAuthStateAsync(dashboardUrl, user))
        );
    }
}
