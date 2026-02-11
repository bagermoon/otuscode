using Mediator;

using RestoRate.Contracts.Common;
using RestoRate.Contracts.Review.Dtos;
using RestoRate.ReviewService.Application.UseCases.Reviews.GetAll;
using RestoRate.SharedKernel.Enums;

namespace RestoRate.ReviewService.Api.Endpoints.Reviews;

internal static class GetAllReviewsEndpoint
{
    public static RouteHandlerBuilder MapGetAllReviews(this RouteGroupBuilder group)
    {
        return group.MapGet("/", async (
            ISender sender,
            CancellationToken ct,
            int pageNumber = 1,
            int pageSize = 20,
            ReviewStatus[]? statuses = null) =>
        {
            var query = new GetAllReviewsQuery(pageNumber, pageSize, statuses);
            var result = await sender.Send(query, ct);
            return result;
        })
        .WithName("GetAllReviews")
        .WithSummary("Получить список отзывов")
        .WithDescription("Получает список отзывов с пагинацией и фильтрацией по статусам")
        .Produces<PagedResult<ReviewDto>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
