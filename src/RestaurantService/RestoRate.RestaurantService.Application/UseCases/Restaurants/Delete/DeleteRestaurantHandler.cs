using Ardalis.Result;

using Mediator;

using RestoRate.RestaurantService.Domain.Interfaces;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.Delete;

public sealed class DeleteRestaurantHandler(
    IRestaurantService restaurantService)
    : ICommandHandler<DeleteRestaurantCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeleteRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await restaurantService.DeleteRestaurantAsync(request.RestaurantId);

            return result.Status switch
            {
                ResultStatus.NotFound => Result.NotFound(),
                ResultStatus.Ok => Result.NoContent(),
                _ => Result.Error(result.Errors.FirstOrDefault() ?? "Неизвестная ошибка")
            };
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
