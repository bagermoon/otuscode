using System;
using System.Text;

using Ardalis.Result;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace RestoRate.ServiceDefaults;

public static class ResultExtensions
{
    public static Microsoft.AspNetCore.Http.IResult ToMinimalApiResult<T>(this Result<T> result)
    {
        return ((Ardalis.Result.IResult)result).ToMinimalApiResult();
    }

    public static Microsoft.AspNetCore.Http.IResult ToMinimalApiResult(this Result result)
    {
        return ((Ardalis.Result.IResult)result).ToMinimalApiResult();
    }

    internal static Microsoft.AspNetCore.Http.IResult ToMinimalApiResult(this Ardalis.Result.IResult result)
    {
        return result.Status switch
        {
            ResultStatus.Ok => (result is Result) ? Results.Ok() : Results.Ok(result.GetValue()),
            ResultStatus.Created => Results.Created(result.Location, result.GetValue()),
            ResultStatus.NoContent => Results.NoContent(),
            ResultStatus.NotFound => NotFoundEntity(result),
            ResultStatus.Unauthorized => UnAuthorized(result),
            ResultStatus.Forbidden => Forbidden(result),
            ResultStatus.Invalid => Invalid(result),
            ResultStatus.Error => UnprocessableEntity(result),
            ResultStatus.Conflict => ConflictEntity(result),
            ResultStatus.Unavailable => UnavailableEntity(result),
            ResultStatus.CriticalError => CriticalEntity(result),
            _ => throw new NotSupportedException($"Result {result.Status} conversion is not supported."),
        };
    }

    private static Microsoft.AspNetCore.Http.IResult UnprocessableEntity(Ardalis.Result.IResult result)
    {
        StringBuilder stringBuilder = new StringBuilder("Next error(s) occurred:");
        foreach (string error in result.Errors)
        {
            stringBuilder.Append("* ").Append(error).AppendLine();
        }

        return Results.Problem(new ProblemDetails
        {
            Title = "Something went wrong.",
            Detail = stringBuilder.ToString(),
            Status = StatusCodes.Status422UnprocessableEntity
        });
    }

    private static Microsoft.AspNetCore.Http.IResult NotFoundEntity(Ardalis.Result.IResult result)
    {
        StringBuilder stringBuilder = new StringBuilder("Next error(s) occurred:");
        if (result.Errors.Any())
        {
            foreach (string error in result.Errors)
            {
                stringBuilder.Append("* ").Append(error).AppendLine();
            }

            return Results.Problem(new ProblemDetails
            {
                Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status404NotFound),
                Detail = stringBuilder.ToString(),
                Status = StatusCodes.Status404NotFound
            });
        }

        return Results.NotFound();
    }

    private static Microsoft.AspNetCore.Http.IResult ConflictEntity(Ardalis.Result.IResult result)
    {
        StringBuilder stringBuilder = new StringBuilder("Next error(s) occurred:");
        if (result.Errors.Any())
        {
            foreach (string error in result.Errors)
            {
                stringBuilder.Append("* ").Append(error).AppendLine();
            }

            return Results.Problem(new ProblemDetails
            {
                Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status409Conflict),
                Detail = stringBuilder.ToString(),
                Status = StatusCodes.Status409Conflict
            });
        }

        return Results.Problem(new ProblemDetails
        {
            Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status409Conflict),
            Status = StatusCodes.Status409Conflict
        });
    }

    private static Microsoft.AspNetCore.Http.IResult CriticalEntity(Ardalis.Result.IResult result)
    {
        StringBuilder stringBuilder = new StringBuilder("Next error(s) occurred:");
        if (result.Errors.Any())
        {
            foreach (string error in result.Errors)
            {
                stringBuilder.Append("* ").Append(error).AppendLine();
            }

            return Results.Problem(new ProblemDetails
            {
                Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status500InternalServerError),
                Detail = stringBuilder.ToString(),
                Status = StatusCodes.Status500InternalServerError
            });
        }

        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }

    private static Microsoft.AspNetCore.Http.IResult UnavailableEntity(Ardalis.Result.IResult result)
    {
        StringBuilder stringBuilder = new StringBuilder("Next error(s) occurred:");
        if (result.Errors.Any())
        {
            foreach (string error in result.Errors)
            {
                stringBuilder.Append("* ").Append(error).AppendLine();
            }

            return Results.Problem(new ProblemDetails
            {
                Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status503ServiceUnavailable),
                Detail = stringBuilder.ToString(),
                Status = StatusCodes.Status503ServiceUnavailable
            });
        }

        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }

    private static Microsoft.AspNetCore.Http.IResult Forbidden(Ardalis.Result.IResult result)
    {
        StringBuilder stringBuilder = new StringBuilder("Next error(s) occurred:");
        if (result.Errors.Any())
        {
            foreach (string error in result.Errors)
            {
                stringBuilder.Append("* ").Append(error).AppendLine();
            }

            return Results.Problem(new ProblemDetails
            {
                Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status403Forbidden),
                Detail = stringBuilder.ToString(),
                Status = StatusCodes.Status403Forbidden
            });
        }

        return Results.Forbid();
    }

    private static Microsoft.AspNetCore.Http.IResult Invalid(Ardalis.Result.IResult result)
    {
        var errors = result.ValidationErrors.GroupBy(e => e.Identifier).ToDictionary(
            g => g.Key ?? "",
            g => g.Select(e => e.ErrorMessage ?? string.Empty).ToArray());

        return Results.ValidationProblem(errors);
    }

    private static Microsoft.AspNetCore.Http.IResult UnAuthorized(Ardalis.Result.IResult result)
    {
        StringBuilder stringBuilder = new StringBuilder("Next error(s) occurred:");
        if (result.Errors.Any())
        {
            foreach (string error in result.Errors)
            {
                stringBuilder.Append("* ").Append(error).AppendLine();
            }

            return Results.Problem(new ProblemDetails
            {
                Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status401Unauthorized),
                Detail = stringBuilder.ToString(),
                Status = StatusCodes.Status401Unauthorized
            });
        }

        return Results.Unauthorized();
    }
}
