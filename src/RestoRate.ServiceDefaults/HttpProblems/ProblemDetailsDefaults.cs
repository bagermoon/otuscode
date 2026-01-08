using System.Reflection;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace RestoRate.ServiceDefaults.HttpProblems;

internal static class ProblemDetailsDefaults
{
    private static Dictionary<int, (string Type, string Title)>? TryGetFrameworkProblemDefaults()
    {
        var defaultsType = AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType("Microsoft.AspNetCore.Http.ProblemDetailsDefaults", false))
            .FirstOrDefault(t => t != null);
        if (defaultsType == null) return null;

        var field = defaultsType.GetField("Defaults", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (field == null) return null;

        return field.GetValue(null) as Dictionary<int, (string Type, string Title)>;
    }

    public static readonly Dictionary<int, (string Type, string Title)> Defaults;

    static ProblemDetailsDefaults()
    {
        Defaults = TryGetFrameworkProblemDefaults() ?? new Dictionary<int, (string Type, string Title)>();
    }

    public static void Apply(ProblemDetails problemDetails, int? statusCode)
    {
        // We allow StatusCode to be specified either on ProblemDetails or on the ObjectResult and use it to configure the other.
        // This lets users write <c>return Conflict(new Problem("some description"))</c>
        // or <c>return Problem("some-problem", 422)</c> and have the response have consistent fields.
        if (problemDetails.Status is null)
        {
            if (statusCode is not null)
            {
                problemDetails.Status = statusCode;
            }
            else
            {
                problemDetails.Status = problemDetails is HttpValidationProblemDetails ?
                    StatusCodes.Status400BadRequest :
                    StatusCodes.Status500InternalServerError;
            }
        }

        var status = problemDetails.Status.GetValueOrDefault();
        if (Defaults.TryGetValue(status, out var defaults))
        {
            problemDetails.Title ??= defaults.Title;
            problemDetails.Type ??= defaults.Type;
        }
        else if (problemDetails.Title is null)
        {
            var reasonPhrase = ReasonPhrases.GetReasonPhrase(status);
            if (!string.IsNullOrEmpty(reasonPhrase))
            {
                problemDetails.Title = reasonPhrase;
            }
        }
    }
}
