using System;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using RestoRate.ServiceDefaults.HttpProblems;

namespace RestoRate.ServiceDefaults;

public static class ProblemDetailsExtensions
{
    public static IServiceCollection AddProblemDetailsDefaults(this IServiceCollection services)
    {
        services.AddProblemDetails(options => options.CustomizeProblemDetails = CustomizeProblemDetails);

        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IProblemDetailsWriter, FallbackProblemDetailsWriter>()
        );

        // Register default exception->ProblemDetails writers
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IExceptionProblemDetailsWriter, FluentValidationExceptionProblemDetailsWriter>());

        return services;
    }

    private static void CustomizeProblemDetails(ProblemDetailsContext context)
    {
        var httpContext = context.HttpContext;

        var exception = httpContext.Features.Get<IExceptionHandlerFeature>()?.Error;
        var statusCodePagesEnabled = httpContext.Features.Get<IStatusCodePagesFeature>()?.Enabled == true;

        if (statusCodePagesEnabled || exception is not null)
        {
            httpContext.Response.Headers.TryAdd("Cache-Control", "no-store, no-cache");
        }

        if (exception is null) return;

        var writer = httpContext.RequestServices
            .GetServices<IExceptionProblemDetailsWriter>()
            .FirstOrDefault(w => w.CanWrite(exception));

        if (writer is null) return;

        var problemDetails = context.ProblemDetails;
        var initialType = problemDetails.Type;

        writer.Write(context, exception);

        if (problemDetails.Type == initialType)
        {
            problemDetails.Type = null;
        }

        ProblemDetailsDefaults.Apply(problemDetails, httpContext.Response.StatusCode);
        problemDetails.Type ??= initialType;
        httpContext.Response.StatusCode = problemDetails.Status ?? httpContext.Response.StatusCode;
    }
}
