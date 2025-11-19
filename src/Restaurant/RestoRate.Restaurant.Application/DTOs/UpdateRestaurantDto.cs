namespace RestoRate.Restaurant.Application.DTOs;

public record UpdateRestaurantDto(
    int RestaurantId,
    string Name,
    string Description,
    string PhoneNumber,
    string Email,
    string FullAddress,
    string House,
    double Latitude,
    double Longitude,
    DayOfWeek DayOfWeek,
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    string CuisineType,
    decimal AverageCheckAmount,
    string AverageCheckCurrency,
    string Tag);
