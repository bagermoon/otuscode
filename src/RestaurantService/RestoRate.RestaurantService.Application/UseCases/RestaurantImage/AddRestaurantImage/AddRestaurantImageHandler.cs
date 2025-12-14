using Ardalis.Result;
using Ardalis.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;
using RestaurantEntity = RestoRate.RestaurantService.Domain.RestaurantAggregate.Restaurant;

namespace RestoRate.RestaurantService.Application.UseCases.RestaurantImage.AddRestaurantImage;

public sealed class AddRestaurantImageHandler(
    IRepository<RestaurantEntity> repository,
    ILogger<AddRestaurantImageHandler> logger)
    : ICommandHandler<AddRestaurantImageCommand, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(
        AddRestaurantImageCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Добавление изображения к ресторану {RestaurantId}: {Url}",
            request.RestaurantId,
            request.Url);

        try
        {
            var restaurant = await repository.GetByIdAsync(request.RestaurantId, cancellationToken);
            if (restaurant == null)
            {
                logger.LogWarning("Ресторан {RestaurantId} не найден", request.RestaurantId);
                return Result<Guid>.NotFound();
            }

            var displayOrder = restaurant.Images.Count;
            var addedImage = restaurant.AddImage(request.Url, request.AltText, displayOrder, request.IsPrimary);

            await repository.UpdateAsync(restaurant, cancellationToken);

            logger.LogInformation(
                "Изображение {ImageId} добавлено к ресторану {RestaurantId}",
                addedImage.Id,
                request.RestaurantId);

            return Result<Guid>.Success(addedImage.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при добавлении изображения");
            return Result<Guid>.Error(ex.Message);
        }
    }
}
