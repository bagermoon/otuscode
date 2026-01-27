using Ardalis.Result;

using Mediator;

using RestoRate.Contracts.Review.Dtos;
using RestoRate.ReviewService.Application.UseCases.Reviews.GetById;

namespace RestoRate.ReviewService.Api.Endpoints.Reviews;

internal static class GetReviewByIdEndpoint
{
    // Регистрирует endpoint для получения отзыва по Id
    public static RouteGroupBuilder MapGetReviewById(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetReviewByIdQuery(id), ct);
            return result;
        })
        .WithName("GetReviewById")
        .WithSummary("Получить отзыв по ID")
        .Produces<ReviewDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        return group;
    }
}
