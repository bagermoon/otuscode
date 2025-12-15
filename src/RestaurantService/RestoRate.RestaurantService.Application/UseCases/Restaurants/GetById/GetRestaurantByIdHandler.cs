using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using Microsoft.Extensions.Logging;

using RestoRate.RestaurantService.Application.DTOs;
using RestoRate.RestaurantService.Domain.RestaurantAggregate.Specifications;
using RestoRate.SharedKernel.Enums;

using RestaurantEntity = RestoRate.RestaurantService.Domain.RestaurantAggregate.Restaurant;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.GetById;

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
                restaurant.Description,
                restaurant.PhoneNumber.ToString(),
                restaurant.Email.Address,
                new AddressDto(restaurant.Address.FullAddress, restaurant.Address.House),
                new LocationDto(restaurant.Location.Latitude, restaurant.Location.Longitude),
                new OpenHoursDto(restaurant.OpenHours.DayOfWeek, restaurant.OpenHours.OpenTime, restaurant.OpenHours.CloseTime),
                new MoneyDto(restaurant.AverageCheck.Amount, restaurant.AverageCheck.Currency),
                restaurant.RestaurantStatus.Name,
                restaurant.CuisineTypes.Select(ct => ct.CuisineType.Name).ToList(),
                restaurant.Tags.Select(t => t.Tag.Name).ToList(),
                restaurant.Images.Select(img => new RestaurantImageDto(
                    img.Id,
                    img.Url,
                    img.AltText,
                    img.DisplayOrder,
                    img.IsPrimary
                )).ToList()
            );

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
