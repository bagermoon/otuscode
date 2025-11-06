using System.Diagnostics;

namespace RestoRate.ServiceDefaults;

public static class ActivityExtensions
{
    // See https://opentelemetry.io/docs/specs/semconv/exceptions/exceptions-spans/
    public static void SetExceptionTags(this Activity activity, Exception ex)
    {
        if (activity is null)
        {
            return;
        }

        activity.AddTag("exception.message", ex.Message);
        activity.AddTag("exception.stacktrace", ex.ToString());
        activity.AddTag("exception.type", ex.GetType().FullName);
        activity.SetStatus(ActivityStatusCode.Error);
    }
}
