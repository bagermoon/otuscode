namespace RestoRate.Restaurant.Application.DTOs;

public record CreateRestaurantDto(
    string Name,
    string Description,
    string PhoneNumber,
    string Email,
    double Latitude,
    double Longitude,
    decimal AverageCheckAmount,
    string AverageCheckCurrency,
    string Tag);
