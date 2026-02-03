using RestoRate.Contracts.Common.Dtos;

namespace RestoRate.Contracts.Restaurant.DTOs.CRUD;

public record CreateRestaurantDto(
    string Name,
    string? Description,
    string PhoneNumber,
    string Email,
    AddressDto Address,
    LocationDto Location,
    List<OpenHoursDto> OpenHours,
    MoneyDto AverageCheck,
    IReadOnlyCollection<string> CuisineTypes,
    IReadOnlyCollection<string> Tags,
    IReadOnlyCollection<CreateRestaurantImageDto> Images);
