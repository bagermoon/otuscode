using RestoRate.Abstractions.Messaging;

namespace RestoRate.Contracts.Review.Events;

/// <summary>
/// Интеграционное событие: отзыв подтверждён (Approved).
/// Используется для финальной фиксации рейтинга в RatingService.
/// </summary>
/// <param name="ReviewId">Идентификатор отзыва.</param>
public sealed record ReviewApprovedEvent(Guid ReviewId) : IIntegrationEvent;
