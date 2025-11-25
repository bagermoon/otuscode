using System.Reflection;

namespace RestoRate.E2ETests.Base;

public class WithTestNameAttribute : BeforeAfterTestAttribute
{
    public static bool TrackingEnabled { get; set; } = true;
    private static readonly AsyncLocal<string> _currentTestName = new();
    private static readonly AsyncLocal<string> _currentClassName = new();

    public static string CurrentTestName
    {
        get => _currentTestName.Value ?? string.Empty;
        set => _currentTestName.Value = value;
    }
    public static string CurrentClassName
    {
        get => _currentClassName.Value ?? string.Empty;
        set => _currentClassName.Value = value;
    }

    public override void Before(MethodInfo methodInfo, IXunitTest test)
    {
        if (TrackingEnabled)
        {
            CurrentTestName = methodInfo.Name;
            CurrentClassName = methodInfo.DeclaringType?.Name ?? string.Empty;
            Console.WriteLine($"[Test Tracking] {CurrentClassName}.{CurrentTestName} started.");
        }
    }

    public override void After(MethodInfo methodInfo, IXunitTest test)
    {
        if (TrackingEnabled)
        {
            Console.WriteLine($"[Test Tracking] {CurrentClassName}.{CurrentTestName} finished.");
        }
    }
}
