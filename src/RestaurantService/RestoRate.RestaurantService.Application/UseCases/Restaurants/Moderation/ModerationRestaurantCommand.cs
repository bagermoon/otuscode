using Ardalis.Result;

using Mediator;

using RestoRate.Contracts.Restaurant;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.Moderation;

/// <summary>
/// Command to moderate a restaurant (approve or reject).
/// </summary>
public sealed record ModerationRestaurantCommand(
    Guid RestaurantId,
    RestaurantStatus Status,
    string? Reason
) : ICommand<Result>;
