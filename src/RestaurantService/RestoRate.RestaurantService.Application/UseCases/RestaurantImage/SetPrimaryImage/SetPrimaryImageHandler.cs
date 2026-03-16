using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using Microsoft.Extensions.Logging;

using RestaurantEntity = RestoRate.RestaurantService.Domain.RestaurantAggregate.Restaurant;

namespace RestoRate.RestaurantService.Application.UseCases.RestaurantImage.SetPrimaryImage;

public sealed class SetPrimaryImageHandler(
    IRepository<RestaurantEntity> repository,
    ILogger<SetPrimaryImageHandler> logger)
    : ICommandHandler<SetPrimaryImageCommand, Result>
{
    public async ValueTask<Result> Handle(
        SetPrimaryImageCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogSettingPrimary(request.ImageId, request.RestaurantId);

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

            restaurant.SetPrimaryImage(request.ImageId);

            await repository.UpdateAsync(restaurant, cancellationToken);

            logger.LogPrimarySet(request.ImageId, request.RestaurantId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogSetPrimaryError(ex);
            return Result.Error(ex.Message);
        }
    }
}
