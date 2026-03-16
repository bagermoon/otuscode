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
        logger.LogRemoving(request.ImageId, request.RestaurantId);

        try
        {
            var restaurant = await repository.GetByIdAsync(request.RestaurantId, cancellationToken);
            if (restaurant == null)
            {
                logger.LogRestaurantNotFound(request.RestaurantId);
                return Result.NotFound();
            }

            var image = restaurant.Images.FirstOrDefault(img => img.Id == request.ImageId);
            if (image == null)
            {
                logger.LogImageNotFound(request.ImageId, request.RestaurantId);
                return Result.NotFound();
            }

            restaurant.RemoveImage(request.ImageId);

            await repository.UpdateAsync(restaurant, cancellationToken);

            logger.LogRemoved(request.ImageId, request.RestaurantId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogRemoveError(ex);
            return Result.Error(ex.Message);
        }
    }
}
