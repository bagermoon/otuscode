using Ardalis.Result;

using Mediator;

using Microsoft.Extensions.Logging;

using NodaMoney;

using RestoRate.RestaurantService.Domain.Interfaces;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.Update;

public sealed class UpdateRestaurantHandler(
    IRestaurantService restaurantService,
    ITagsService tagsService,
    ILogger<UpdateRestaurantHandler> logger)
    : ICommandHandler<UpdateRestaurantCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogUpdating(request.Dto.RestaurantId);

        try
        {
            var phoneNumber = new PhoneNumber("+7", request.Dto.PhoneNumber);
            var email = new Email(request.Dto.Email);
            var address = new Address(request.Dto.Address.FullAddress, request.Dto.Address.House);
            var location = new Location(request.Dto.Location.Latitude, request.Dto.Location.Longitude);

            var openHours = request.Dto.OpenHours
                .Select(oh => new OpenHours(oh.DayOfWeek, oh.OpenTime, oh.CloseTime, oh.IsClosed))
                .ToList();

            var averageCheck = new Money(request.Dto.AverageCheck.Amount, Currency.FromCode(request.Dto.AverageCheck.Currency));

            var cuisineTypes = request.Dto.CuisineTypes
                .Select(ct => CuisineType.FromName(ct))
                .ToList();

            var restaurantTags = await tagsService.ConvertToTagsAsync(
                request.Dto.Tags ?? [], cancellationToken);

            var result = await restaurantService.UpdateRestaurantAsync(
                request.Dto.RestaurantId,
                request.Dto.Name,
                request.Dto.Description,
                phoneNumber,
                email,
                address,
                location,
                openHours,
                averageCheck,
                cuisineTypes,
                restaurantTags);

            if (result.Status == ResultStatus.NotFound)
            {
                logger.LogNotFound(request.Dto.RestaurantId);
                return Result.NotFound();
            }

            if (result.Status != ResultStatus.Ok)
            {
                logger.LogUpdateFailed();
                return Result.Error(result.Errors.FirstOrDefault() ?? "Неизвестная ошибка");
            }

            logger.LogUpdated(request.Dto.RestaurantId);
            return Result.NoContent();
        }
        catch (Exception ex)
        {
            logger.LogUpdateError(ex);
            return Result.Error(ex.Message);
        }
    }
}
