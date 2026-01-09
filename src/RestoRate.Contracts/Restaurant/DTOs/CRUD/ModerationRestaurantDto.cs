namespace RestoRate.Contracts.Restaurant.DTOs.CRUD;

public record ModerationRestaurantDto
{
    public RestaurantStatus Status { get; init; }
    public string? Reason { get; init; } // Optional reason for rejection
}
