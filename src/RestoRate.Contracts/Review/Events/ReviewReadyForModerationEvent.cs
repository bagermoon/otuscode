using RestoRate.Abstractions.Messaging;

namespace RestoRate.Contracts.Review.Events;

/// <summary>
/// Emitted by ReviewService once the restaurant is validated and the review can be sent to moderation.
/// </summary>
public sealed record ReviewReadyForModerationEvent(
    Guid ReviewId,
    Guid RestaurantId) : IIntegrationEvent;
