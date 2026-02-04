using Microsoft.AspNetCore.Builder;

namespace RestoRate.ReviewService.Api.Endpoints.Reviews;

public static class ReviewsEndpoints
{
    // Регистрирует группу endpoints для работы с отзывами
    public static RouteGroupBuilder MapReviewsEndpoints(this IEndpointRouteBuilder app, string prefix = "reviews")
    {
        var group = app.MapGroup($"/{prefix}");
        group.MapCreateReview(); // Подключаем endpoint создания отзыва
        group.MapGetAllReviews(); // Подключаем endpoint получения списка отзывов
        group.MapGetReviewById(); // Подключаем endpoint получения отзыва по Id
        return group;
    }
}
