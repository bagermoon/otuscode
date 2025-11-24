namespace RestoRate.Restaurant.Application.DTOs;

public record OpenHoursDto(
    DayOfWeek DayOfWeek,
    TimeOnly OpenTime,
    TimeOnly CloseTime
);
