using Ardalis.Result;
using Ardalis.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;
using RestaurantEntity = RestoRate.Restaurant.Domain.RestaurantAggregate.Restaurant;

namespace RestoRate.Restaurant.Application.UseCases.RestaurantImage.SetPrimaryImage;

public sealed class SetPrimaryImageHandler(
    IRepository<RestaurantEntity> repository,
    ILogger<SetPrimaryImageHandler> logger)
    : ICommandHandler<SetPrimaryImageCommand, Result>
{
    public async ValueTask<Result> Handle(
        SetPrimaryImageCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Установка главного изображения {ImageId} для ресторана {RestaurantId}",
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

            restaurant.SetPrimaryImage(request.ImageId);

            await repository.UpdateAsync(restaurant, cancellationToken);

            logger.LogInformation(
                "Изображение {ImageId} установлено как главное для ресторана {RestaurantId}",
                request.ImageId,
                request.RestaurantId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при установке главного изображения");
            return Result.Error(ex.Message);
        }
    }
}
