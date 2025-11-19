using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;
using Microsoft.Extensions.Logging;

using RestoRate.Restaurant.Application.DTOs;
using RestoRate.Restaurant.Domain.RestaurantAggregate.Specifications;
using RestaurantEntity = RestoRate.Restaurant.Domain.RestaurantAggregate.Restaurant;

namespace RestoRate.Restaurant.Application.UseCases.GetById;

public sealed class GetRestaurantByIdHandler(
    IReadRepository<RestaurantEntity> readRepository,
    ILogger<GetRestaurantByIdHandler> logger)
    : IQueryHandler<GetRestaurantByIdQuery, Result<RestaurantDto>>
{
    public async ValueTask<Result<RestaurantDto>> Handle(
        GetRestaurantByIdQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Обработка запроса получения ресторана: ID {RestaurantId}", request.RestaurantId);

        try
        {
            var spec = new GetRestaurantByIdSpec(request.RestaurantId);
            var restaurant = await readRepository.FirstOrDefaultAsync(spec, cancellationToken);

            if (restaurant == null)
            {
                logger.LogWarning("Ресторан не найден: ID {RestaurantId}", request.RestaurantId);
                return Result.NotFound();
            }

            var dto = new RestaurantDto(
                restaurant.Id,
                restaurant.Name,
                restaurant.Description ?? string.Empty,
                restaurant.PhoneNumber.ToString(),
                restaurant.Email.Address,
                restaurant.Address.FullAddress,
                restaurant.Address.House,
                restaurant.Location.Latitude,
                restaurant.Location.Longitude,
                restaurant.OpenHours.DayOfWeek,
                restaurant.OpenHours.OpenTime,
                restaurant.OpenHours.CloseTime,
                restaurant.CuisineType.ToString(),
                restaurant.AverageCheck.Amount,
                restaurant.AverageCheck.Currency,
                restaurant.Tag.Name);

            logger.LogInformation("Ресторан найден успешно: ID {RestaurantId}", request.RestaurantId);
            return Result<RestaurantDto>.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении ресторана");
            return Result<RestaurantDto>.Error(ex.Message);
        }
    }
}
