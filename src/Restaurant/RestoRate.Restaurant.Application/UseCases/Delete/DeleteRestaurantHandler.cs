using Ardalis.Result;

using Mediator;

using Microsoft.Extensions.Logging;

using RestoRate.Restaurant.Domain.Interfaces;

namespace RestoRate.Restaurant.Application.UseCases.Delete;

internal class DeleteRestaurantHandler(
    IDeleteRestaurantService deleteService,
    ILogger<DeleteRestaurantHandler> logger)
    : IRequestHandler<DeleteRestaurantCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeleteRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Обработка команды удаления ресторана: ID {RestaurantId}", request.RestaurantId);

        try
        {
            var result = await deleteService.DeleteRestaurant(request.RestaurantId);

            if (result.Status != ResultStatus.Ok)
            {
                logger.LogWarning("Не удалось удалить ресторан");
                return Result.Error(result.Errors.FirstOrDefault() ?? "Неизвестная ошибка");
            }

            logger.LogInformation("Ресторан удален успешно: ID {RestaurantId}", request.RestaurantId);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении ресторана");
            return Result.Error(ex.Message);
        }
    }
}
