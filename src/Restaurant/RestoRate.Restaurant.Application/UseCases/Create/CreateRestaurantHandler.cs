using Ardalis.Result;
using Mediator;
using Microsoft.Extensions.Logging;
using RestoRate.Restaurant.Application.DTOs;
using RestoRate.Restaurant.Domain.Interfaces;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;

namespace RestoRate.Restaurant.Application.UseCases.Create;

internal class CreateRestaurantHandler(
    ICreateRestaurantService createService,
    ILogger<CreateRestaurantHandler> logger)
    : IRequestHandler<CreateRestaurantCommand, Result<RestaurantDto>>
{
    public async ValueTask<Result<RestaurantDto>> Handle(
        CreateRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Обработка команды создания ресторана: {RestaurantName}", request.Dto.Name);

        try
        {
            var phoneNumber = new PhoneNumber("+7", request.Dto.PhoneNumber);
            var email = new Email(request.Dto.Email);
            var location = new Location(request.Dto.Latitude, request.Dto.Longitude);
            var averageCheck = new Money(request.Dto.AverageCheckAmount, request.Dto.AverageCheckCurrency);
            var tag = RestaurantTag.FromName(request.Dto.Tag);

            var result = await createService.CreateRestaurant(
                request.Dto.Name,
                request.Dto.Description,
                phoneNumber,
                email,
                location,
                averageCheck,
                tag);

            if (result.Status != ResultStatus.Ok)
            {
                logger.LogWarning("Не удалось создать ресторан");
                return Result<RestaurantDto>.Error(result.Errors.FirstOrDefault() ?? "Неизвестная ошибка");
            }

            var dto = new RestaurantDto(
                result.Value,
                request.Dto.Name,
                request.Dto.Description,
                phoneNumber.ToString(),
                email.Address,
                location.Latitude,
                location.Longitude,
                averageCheck.Amount,
                averageCheck.Currency,
                tag.Name);

            logger.LogInformation("Ресторан создан успешно: ID {RestaurantId}", result.Value);
            return Result<RestaurantDto>.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при создании ресторана");
            return Result<RestaurantDto>.Error(ex.Message);
        }
    }
}
