namespace RestoRate.Restaurant.Application.DTOs;

public record RestaurantDto(
    Guid RestaurantId,
    string Name,
    string? Description,
    string PhoneNumber,
    string Email,
    AddressDto Address,
    LocationDto Location,
    OpenHoursDto OpenHours,
    MoneyDto AverageCheck,
    IReadOnlyCollection<string> CuisineTypes,
    IReadOnlyCollection<string> Tags,
    IReadOnlyCollection<RestaurantImageDto> Images);
