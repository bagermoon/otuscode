using FluentValidation;

using Microsoft.AspNetCore.Http;

namespace RestoRate.ServiceDefaults.HttpProblems;

public sealed class FluentValidationExceptionProblemDetailsWriter : IExceptionProblemDetailsWriter
{
    public bool CanWrite(Exception exception) => exception is ValidationException;

    public void Write(ProblemDetailsContext context, Exception exception)
    {
        var httpContext = context.HttpContext;
        var validationException = (ValidationException)exception;

        context.ProblemDetails.Status = StatusCodes.Status400BadRequest;
        context.ProblemDetails.Title = "One or more validation errors occurred.";

        var errors = validationException.Errors
            .GroupBy(e => e.PropertyName ?? "")
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage ?? string.Empty).ToArray());

        context.ProblemDetails.Extensions["errors"] = errors;
    }
}
