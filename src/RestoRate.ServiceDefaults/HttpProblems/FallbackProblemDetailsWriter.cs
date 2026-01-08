using System.Diagnostics;
using System.Text;
using System.Text.Json;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace RestoRate.ServiceDefaults.HttpProblems;

internal sealed class FallbackProblemDetailsWriter(
    IServiceProvider sp,
    IOptions<ProblemDetailsOptions> options,
    IOptions<JsonOptions> jsonOptions
    ) : IProblemDetailsWriter
{
    private readonly IServiceProvider _sp = sp;
    private readonly ProblemDetailsOptions _options = options.Value;
    private readonly JsonSerializerOptions _serializerOptions = jsonOptions.Value.SerializerOptions;

    public bool CanWrite(ProblemDetailsContext context)
    {
        var writers = _sp.GetServices<IProblemDetailsWriter>()
        .Where(writer => typeof(FallbackProblemDetailsWriter) != writer.GetType());

        if (context.HttpContext.Response.HasStarted) return false;
        return !writers?.Any(w => w.CanWrite(context)) ?? true;
    }

    public ValueTask WriteAsync(ProblemDetailsContext context)
    {
        var httpContext = context.HttpContext;

        var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
        var traceIdKeyName = _serializerOptions.PropertyNamingPolicy?.ConvertName("traceId") ?? "traceId";
        context.ProblemDetails.Extensions[traceIdKeyName] = traceId;

        _options.CustomizeProblemDetails?.Invoke(context);

        httpContext.Response.Headers.TryAdd("Vary", "Accept, Accept-Language");
        httpContext.Response.ContentType = "text/plain";

        return new ValueTask(httpContext.Response.WriteAsync(
            BuildResponseBody(context.ProblemDetails)));
    }

    private static string BuildResponseBody(Microsoft.AspNetCore.Mvc.ProblemDetails problemDetails)
    {
        StringBuilder stringBuilder = new();
        if (problemDetails.Type is not null)
        {
            stringBuilder.AppendLine($"type: {problemDetails.Type}");
        }
        if (problemDetails.Title is not null)
        {
            stringBuilder.AppendLine($"title: {problemDetails.Title}");
        }
        stringBuilder.AppendLine($"status: {problemDetails.Status}");
        if (problemDetails.Detail is not null)
        {
            stringBuilder.AppendLine($"detail: {problemDetails.Detail}");
        }
        if (problemDetails.Instance is not null)
        {
            stringBuilder.AppendLine($"instance: {problemDetails.Instance}");
        }
        foreach (var extension in problemDetails.Extensions)
        {
            stringBuilder.AppendLine($"{extension.Key}: {extension.Value}");
        }

        if (problemDetails is HttpValidationProblemDetails validationProblemDetails)
        {
            stringBuilder.AppendLine($"errors:");
            foreach (var error in validationProblemDetails.Errors)
            {
                stringBuilder.AppendLine($"\t{error.Key}: {string.Join(", ", error.Value)}");
            }
        }

        return stringBuilder.ToString();
    }
}
