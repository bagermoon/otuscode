using Ardalis.Result;
using Mediator;

using Microsoft.Extensions.Logging;

using RestoRate.Restaurant.Application.DTOs;
using RestoRate.Restaurant.Domain.Interfaces;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;

namespace RestoRate.Restaurant.Application.UseCases.Update;

public sealed class UpdateRestaurantHandler(
    IRestaurantService restaurantService,
    ILogger<UpdateRestaurantHandler> logger)
    : ICommandHandler<UpdateRestaurantCommand, Result<UpdateRestaurantDto>>
{
    public async ValueTask<Result<UpdateRestaurantDto>> Handle(
        UpdateRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Обработка команды обновления ресторана: ID {RestaurantId}", request.Dto.RestaurantId);

        try
        {
            var phoneNumber = new PhoneNumber("+7", request.Dto.PhoneNumber);
            var email = new Email(request.Dto.Email);
            var location = new Location(request.Dto.Latitude, request.Dto.Longitude);
            var averageCheck = new Money(request.Dto.AverageCheckAmount, request.Dto.AverageCheckCurrency);
            var tag = RestaurantTag.FromName(request.Dto.Tag);

            var result = await restaurantService.UpdateRestaurant(
                request.Dto.RestaurantId,
                request.Dto.Name,
                request.Dto.Description,
                phoneNumber,
                email,
                location,
                averageCheck,
                tag);

            if (result.Status != ResultStatus.Ok)
            {
                logger.LogWarning("Не удалось обновить ресторан");
                return Result.Error(result.Errors.FirstOrDefault() ?? "Неизвестная ошибка");
            }

            logger.LogInformation("Ресторан обновлен успешно: ID {RestaurantId}", request.Dto.RestaurantId);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обновлении ресторана");
            return Result.Error(ex.Message);
        }
    }
}
