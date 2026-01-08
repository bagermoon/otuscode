using Ardalis.Result;

using Mediator;

using Microsoft.Extensions.Logging;

using RestoRate.RestaurantService.Domain.Interfaces;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.Delete;

public sealed class DeleteRestaurantHandler(
    IRestaurantService restaurantService,
    ILogger<DeleteRestaurantHandler> logger)
    : ICommandHandler<DeleteRestaurantCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeleteRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Обработка команды удаления ресторана: ID {RestaurantId}", request.RestaurantId);

        try
        {
            var result = await restaurantService.DeleteRestaurant(request.RestaurantId);

            if (result.Status == ResultStatus.NotFound)
            {
                logger.LogWarning("Ресторан не найден: ID {RestaurantId}", request.RestaurantId);
                return Result.NotFound();
            }

            if (result.Status != ResultStatus.Ok)
            {
                logger.LogWarning("Не удалось удалить ресторан");
                return Result.Error(result.Errors.FirstOrDefault() ?? "Неизвестная ошибка");
            }

            logger.LogInformation("Ресторан удален успешно: ID {RestaurantId}", request.RestaurantId);
            return Result.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении ресторана");
            return Result.Error(ex.Message);
        }
    }
}
