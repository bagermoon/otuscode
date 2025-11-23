namespace RestoRate.Restaurant.Application.DTOs.CRUD;

public record CreateRestaurantImageDto(
    string Url,
    string? AltText = null,
    bool IsPrimary = false
);
