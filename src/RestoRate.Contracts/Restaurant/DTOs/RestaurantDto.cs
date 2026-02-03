using RestoRate.Contracts.Common.Dtos;

namespace RestoRate.Contracts.Restaurant.DTOs;

public record RestaurantDto(
    Guid RestaurantId,
    string Name,
    string? Description,
    string PhoneNumber,
    string Email,
    AddressDto Address,
    LocationDto Location,
    List<OpenHoursDto> OpenHours,
    MoneyDto AverageCheck,
    Guid OwnerId,
    string RestaurantStatus,
    IReadOnlyCollection<string> CuisineTypes,
    IReadOnlyCollection<string> Tags,
    RatingDto? Rating,
    IReadOnlyCollection<RestaurantImageDto> Images);
