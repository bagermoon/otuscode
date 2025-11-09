using RestoRate.Abstractions.Messaging;

namespace RestoRate.Contracts.Moderation.Events;

public sealed record ReviewModeratedEvent(
    Guid ReviewId,
    Guid RestaurantId,
    bool Approved,
    string? Reason,
    Guid ModeratorId) : IIntegrationEvent;
