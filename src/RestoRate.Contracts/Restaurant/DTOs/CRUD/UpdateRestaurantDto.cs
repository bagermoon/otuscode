namespace RestoRate.Contracts.Restaurant.DTOs.CRUD;

public record UpdateRestaurantDto(
    Guid RestaurantId,
    string Name,
    string Description,
    string PhoneNumber,
    string Email,
    AddressDto Address,
    LocationDto Location,
    OpenHoursDto OpenHours,
    MoneyDto AverageCheck,
    IReadOnlyCollection<string> CuisineTypes,
    IReadOnlyCollection<string> Tags);
