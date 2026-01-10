using RestoRate.Contracts.Restaurant.DTOs;
using RestoRate.Contracts.Common.Dtos;
using RestoRate.RestaurantService.Domain.RestaurantAggregate;

namespace RestoRate.RestaurantService.Application.Mappings;

public static class RestaurantDtoExtensions
{
    public static RestaurantDto ToDto(this Restaurant restaurant)
    {
        return new RestaurantDto(
            restaurant.Id,
            restaurant.Name,
            restaurant.Description,
            restaurant.PhoneNumber.ToString(),
            restaurant.Email.Address,
            new AddressDto(restaurant.Address.FullAddress, restaurant.Address.House),
            new LocationDto(restaurant.Location.Latitude, restaurant.Location.Longitude),
            new OpenHoursDto(restaurant.OpenHours.DayOfWeek, restaurant.OpenHours.OpenTime, restaurant.OpenHours.CloseTime),
            restaurant.AverageCheck.ToDto(),
            RestaurantStatus: restaurant.RestaurantStatus.Name,
            restaurant.CuisineTypes.Select(ct => ct.CuisineType.Name).ToList(),
            restaurant.Tags.Select(t => t.Tag.Name).ToList(),
            restaurant.Rating?.ToDto(),
            restaurant.Images
                .OrderBy(img => img.DisplayOrder)
                .Select(img => new RestaurantImageDto(
                    img.Id,
                    img.Url,
                    img.AltText,
                    img.DisplayOrder,
                    img.IsPrimary
                )).ToArray()
        );
    }
}
