namespace RestoRate.Contracts.Restaurant.DTOs.CRUD;

public record CreateRestaurantImageDto(
    string Url,
    string? AltText = null,
    bool IsPrimary = false
);
