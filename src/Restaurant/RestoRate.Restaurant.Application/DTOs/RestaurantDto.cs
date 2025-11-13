namespace RestoRate.Restaurant.Application.DTOs;

public record RestaurantDto(int Id,
    string Name,
    string? Description,
    string PhoneNumber,
    string Email,
    double Latitude,
    double Longitude,
    decimal AverageCheckAmount,
    string AverageCheckCurrency,
    string Tag);
