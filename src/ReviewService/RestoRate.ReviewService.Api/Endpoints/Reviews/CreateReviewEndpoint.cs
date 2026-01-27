using Ardalis.Result;

using Mediator;

using RestoRate.Contracts.Review.Dtos;
using RestoRate.ReviewService.Application.UseCases.Reviews.Create;

namespace RestoRate.ReviewService.Api.Endpoints.Reviews;

internal static class CreateReviewEndpoint
{
    // Регистрирует endpoint для создания отзыва
    public static RouteGroupBuilder MapCreateReview(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateReviewDto dto, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new CreateReviewCommand(dto), ct);
            return result;
        })
        .WithName("CreateReview")
        .WithSummary("Создать отзыв")
        .WithDescription("Создаёт новый агрегат отзыва")
        .Produces<ReviewDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        return group;
    }
}
