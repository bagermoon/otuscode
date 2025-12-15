namespace RestoRate.Contracts.Restaurant.DTOs;

public record RestaurantImageDto(
    Guid ImageId,
    string Url,
    string? AltText,
    int DisplayOrder,
    bool IsPrimary
);
