namespace RestoRate.Contracts.Restaurant.DTOs;

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
    string RestaurantStatus,
    IReadOnlyCollection<string> CuisineTypes,
    IReadOnlyCollection<string> Tags,
    IReadOnlyCollection<RestaurantImageDto> Images);
