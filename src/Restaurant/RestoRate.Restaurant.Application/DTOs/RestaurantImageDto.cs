namespace RestoRate.Restaurant.Application.DTOs;

public record RestaurantImageDto(
    Guid ImageId,
    string Url,
    string? AltText,
    int DisplayOrder,
    bool IsPrimary
);
