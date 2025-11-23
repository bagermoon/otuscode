using System;

using Microsoft.Playwright;

namespace RestoRate.IntegrationTests;

public static class PlaywrightAuthHelper
{
    public static string GetAuthStatePath(string username)
    {
        var authDir = Path.Combine(AppContext.BaseDirectory, ".auth");
        Directory.CreateDirectory(authDir);
        return Path.Combine(authDir, $"{username}_state.json");
    }
    public static async Task SaveAuthStateAsync(string dashboardUrl, string username, string password)
    {
        using var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var context = await browser.NewContextAsync();
        var page = await context.NewPageAsync();

        await page.GotoAsync(dashboardUrl);
        await page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Username or email" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Username or email" }).FillAsync(username);
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).ClickAsync();
        await page.GetByRole(AriaRole.Textbox, new() { Name = "Password" }).FillAsync(password);
        await page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).IsVisibleAsync();

        await context.StorageStateAsync(new()
        {
            Path = GetAuthStatePath(username)
        });

        await browser.CloseAsync();
    }
}
