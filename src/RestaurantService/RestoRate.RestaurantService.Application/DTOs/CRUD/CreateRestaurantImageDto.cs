namespace RestoRate.RestaurantService.Application.DTOs.CRUD;

public record CreateRestaurantImageDto(
    string Url,
    string? AltText = null,
    bool IsPrimary = false
);
