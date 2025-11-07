using Ardalis.Result;

using MediatR;

using Microsoft.Extensions.Logging;
using RestoRate.Restaurant.Domain.Interfaces;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;

namespace RestoRate.Restaurant.Application.UseCases.Update;

internal class UpdateRestaurantHandler(
    IUpdateRestaurantService updateService,
    ILogger<UpdateRestaurantHandler> logger)
    : IRequestHandler<UpdateRestaurantCommand, Result>
{
    public async Task<Result> Handle(
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

            var result = await updateService.UpdateRestaurant(
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
