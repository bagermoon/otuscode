using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using Microsoft.Extensions.Logging;

using RestoRate.Restaurant.Application.DTOs;
using RestoRate.Restaurant.Domain.RestaurantAggregate.Specifications;
using RestoRate.SharedKernel.Enums;

using RestaurantEntity = RestoRate.Restaurant.Domain.RestaurantAggregate.Restaurant;

namespace RestoRate.Restaurant.Application.UseCases.GetAll;

public sealed class GetAllRestaurantsHandler(
    IReadRepository<RestaurantEntity> readRepository,
    ILogger<GetAllRestaurantsHandler> logger)
    : IQueryHandler<GetAllRestaurantsQuery, Result<PagedResult<RestaurantDto>>>
{
    public async ValueTask<Result<PagedResult<RestaurantDto>>> Handle(
        GetAllRestaurantsQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Получение списка ресторанов: страница {Page}, размер {Size}, поиск: {Search}",
            request.PageNumber,
            request.PageSize,
            request.SearchTerm);

        try
        {
            // Создаем Specification с фильтрами
            var spec = new GetAllRestaurantsSpec(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                request.CuisineType,
                request.Tag);

            var restaurants = await readRepository.ListAsync(spec, cancellationToken);
            var totalCount = await readRepository.CountAsync(spec, cancellationToken);

            var dtos = restaurants.Select(restaurant => new RestaurantDto(
                restaurant.Id,
                restaurant.Name,
                restaurant.Description,
                restaurant.PhoneNumber.ToString(),
                restaurant.Email.Address,
                new AddressDto(restaurant.Address.FullAddress, restaurant.Address.House),
                new LocationDto(restaurant.Location.Latitude, restaurant.Location.Longitude),
                new OpenHoursDto(
                    restaurant.OpenHours.DayOfWeek,
                    restaurant.OpenHours.OpenTime,
                    restaurant.OpenHours.CloseTime),
                new MoneyDto(restaurant.AverageCheck.Amount, restaurant.AverageCheck.Currency),
                restaurant.RestaurantStatus.Name,
                restaurant.CuisineTypes.Select(ct => ct.CuisineType.Name).ToList(),
                restaurant.Tags.Select(t => t.Tag.Name).ToList(),
                restaurant.Images
                    .OrderBy(img => img.DisplayOrder)
                    .Select(img => new RestaurantImageDto(
                        img.Id,
                        img.Url,
                        img.AltText,
                        img.DisplayOrder,
                        img.IsPrimary
                    )).ToList()
            )).ToList();

            var result = new PagedResult<RestaurantDto>(
                dtos,
                totalCount,
                request.PageNumber,
                request.PageSize
            );

            logger.LogInformation("Найдено {Count} ресторанов из {Total}", dtos.Count, totalCount);

            return Result<PagedResult<RestaurantDto>>.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении списка ресторанов");
            return Result<PagedResult<RestaurantDto>>.Error(ex.Message);
        }
    }
}
