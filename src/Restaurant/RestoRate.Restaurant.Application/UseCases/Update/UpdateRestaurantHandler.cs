using Ardalis.Result;
using Mediator;
using Microsoft.Extensions.Logging;
using RestoRate.Restaurant.Application.DTOs;
using RestoRate.Restaurant.Application.DTOs.CRUD;
using RestoRate.Restaurant.Domain.Interfaces;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;

namespace RestoRate.Restaurant.Application.UseCases.Update;

public sealed class UpdateRestaurantHandler(
    IRestaurantService restaurantService,
    ILogger<UpdateRestaurantHandler> logger)
    : ICommandHandler<UpdateRestaurantCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Обработка команды обновления ресторана: ID {RestaurantId}", request.Dto.RestaurantId);

        try
        {
            var phoneNumber = new PhoneNumber("+7", request.Dto.PhoneNumber);
            var email = new Email(request.Dto.Email);
            var address = new Address(request.Dto.Address.FullAddress, request.Dto.Address.House);
            var location = new Location(request.Dto.Location.Latitude, request.Dto.Location.Longitude);
            var openHours = new OpenHours(request.Dto.OpenHours.DayOfWeek, request.Dto.OpenHours.OpenTime, request.Dto.OpenHours.CloseTime);
            var averageCheck = new Money(request.Dto.AverageCheck.Amount, request.Dto.AverageCheck.Currency);

            var cuisineTypes = request.Dto.CuisineTypes
                .Select(ct => CuisineType.FromName(ct))
                .ToList();

            var tags = request.Dto.Tags
                .Select(t => Tag.FromName(t))
                .ToList();

            var result = await restaurantService.UpdateRestaurant(
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
                tags);

            if (result.Status != ResultStatus.Ok)
            {
                logger.LogWarning("Не удалось обновить ресторан");
                return Result.Error(result.Errors.FirstOrDefault() ?? "Неизвестная ошибка");
            }

            logger.LogInformation("Ресторан обновлен успешно: ID {RestaurantId}", request.Dto.RestaurantId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обновлении ресторана");
            return Result.Error(ex.Message);
        }
    }
}
