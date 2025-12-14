namespace RestoRate.RestaurantService.Application.DTOs;

public record OpenHoursDto(
    DayOfWeek DayOfWeek,
    TimeOnly OpenTime,
    TimeOnly CloseTime
);
