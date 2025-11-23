using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;
using RestoRate.IntegrationTests.Auth;

namespace RestoRate.Restaurant.IntegrationTests.Base;

[Collection("AspireAppHost collection")]
[WithTestName]
public abstract class BasePageTest(AspireAppHost appHost) : PageTest
{
    protected virtual TestUser User
    {
        get
        {
            // Try to get UserAttribute from class or method
            var attr = GetType().GetCustomAttributes(typeof(UserAttribute), true)
                .OfType<UserAttribute>().FirstOrDefault();
            return attr?.Name ?? TestUser.Anonymous; // Default to Anonymous
        }
    }

    protected AspireAppHost AppHost { get; } = appHost;

    public override BrowserNewContextOptions ContextOptions()
    {
        var options = base.ContextOptions();

        if (User != TestUser.Anonymous)
        {
            options.StorageStatePath = PlaywrightAuthHelper.GetAuthStatePath(User);
        }
        options.BaseURL = AppHost.DashboardUrl;

        return options;
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync().ConfigureAwait(false);
        await Context.Tracing.StartAsync(new()
        {
            Title = $"{WithTestNameAttribute.CurrentClassName}.{WithTestNameAttribute.CurrentTestName}",
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });
    }

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public override async ValueTask DisposeAsync()
    {
        await Context.Tracing.StopAsync(new()
        {
            Path = TestOk() ? Path.Combine(
                AppContext.BaseDirectory,
                "playwright-traces",
                $"{WithTestNameAttribute.CurrentClassName}.{WithTestNameAttribute.CurrentTestName}.zip"
            ) : null
        });
        await base.DisposeAsync().ConfigureAwait(false);
    }
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
}
