namespace RestoRate.RestaurantService.Application.DTOs.CRUD;

public record CreateRestaurantDto(
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
    IReadOnlyCollection<CreateRestaurantImageDto> Images);
