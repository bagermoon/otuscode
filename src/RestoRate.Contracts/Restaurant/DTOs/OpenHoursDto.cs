namespace RestoRate.Contracts.Restaurant.DTOs;

public record OpenHoursDto(
    DayOfWeek DayOfWeek,
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    bool IsClosed
);
