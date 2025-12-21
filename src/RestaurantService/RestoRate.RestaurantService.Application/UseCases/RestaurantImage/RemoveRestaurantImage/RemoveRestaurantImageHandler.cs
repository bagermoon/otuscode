using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using Microsoft.Extensions.Logging;

using RestaurantEntity = RestoRate.RestaurantService.Domain.RestaurantAggregate.Restaurant;

namespace RestoRate.RestaurantService.Application.UseCases.RestaurantImage.RemoveRestaurantImage;

public sealed class RemoveRestaurantImageHandler(
    IRepository<RestaurantEntity> repository,
    ILogger<RemoveRestaurantImageHandler> logger)
    : ICommandHandler<RemoveRestaurantImageCommand, Result>
{
    public async ValueTask<Result> Handle(
        RemoveRestaurantImageCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Удаление изображения {ImageId} из ресторана {RestaurantId}",
            request.ImageId,
            request.RestaurantId);

        try
        {
            var restaurant = await repository.GetByIdAsync(request.RestaurantId, cancellationToken);
            if (restaurant == null)
            {
                logger.LogWarning("Ресторан {RestaurantId} не найден", request.RestaurantId);
                return Result.NotFound();
            }

            var image = restaurant.Images.FirstOrDefault(img => img.Id == request.ImageId);
            if (image == null)
            {
                logger.LogWarning(
                    "Изображение {ImageId} не найдено в ресторане {RestaurantId}",
                    request.ImageId,
                    request.RestaurantId);
                return Result.NotFound();
            }

            restaurant.RemoveImage(request.ImageId);

            await repository.UpdateAsync(restaurant, cancellationToken);

            logger.LogInformation(
                "Изображение {ImageId} удалено из ресторана {RestaurantId}",
                request.ImageId,
                request.RestaurantId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при удалении изображения");
            return Result.Error(ex.Message);
        }
    }
}
