using System;

using Microsoft.Playwright;

namespace RestoRate.IntegrationTests.Auth;

public static class PlaywrightAuthHelper
{
    private static readonly Dictionary<TestUser, (string Username, string Password)> Users = new()
    {
        [TestUser.Admin] = ("admin", "admin"),
        [TestUser.User] = ("user", "user")
        // Add more as needed
    };

    public static async Task SaveAuthStateAsync(string dashboardUrl, TestUser user)
    {
        if (!Users.TryGetValue(user, out var creds))
            throw new ArgumentException($"User '{user}' not found.");

        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync(dashboardUrl);
        await page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Username or email" }).FillAsync(creds.Username);
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync(creds.Password);
        await page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).IsVisibleAsync();

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
        var tasks = Users.Keys.Select(user => SaveAuthStateAsync(dashboardUrl, user));
        await Task.WhenAll(tasks);
    }
}
