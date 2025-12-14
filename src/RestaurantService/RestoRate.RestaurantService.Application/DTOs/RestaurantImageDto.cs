namespace RestoRate.RestaurantService.Application.DTOs;

public record RestaurantImageDto(
    Guid ImageId,
    string Url,
    string? AltText,
    int DisplayOrder,
    bool IsPrimary
);
