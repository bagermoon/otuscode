using System.Diagnostics;

namespace RestoRate.Testing.Common.Helpers;

public static class Eventually
{
    public static async Task SucceedsAsync(
        Func<Task> assertion,
        TimeSpan? timeout = null,
        TimeSpan? delay = null)
    {
        timeout ??= TimeSpan.FromSeconds(3);
        delay ??= TimeSpan.FromMilliseconds(50);

        var stopwatch = Stopwatch.StartNew();
        Exception? last = null;

        while (stopwatch.Elapsed < timeout)
        {
            try
            {
                await assertion();
                return;
            }
            catch (Exception ex)
            {
                last = ex;
                await Task.Delay(delay.Value);
            }
        }

        throw last ?? new TimeoutException("Condition was not met within the timeout.");
    }
}
