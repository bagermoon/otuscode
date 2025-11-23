using Microsoft.Playwright.Xunit.v3;

using RestoRate.Restaurant.IntegrationTests.Tests;

[WithTestName]
public abstract class BasePageTest : PageTest
{
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
