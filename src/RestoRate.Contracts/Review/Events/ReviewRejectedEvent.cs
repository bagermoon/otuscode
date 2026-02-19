using RestoRate.Abstractions.Messaging;

namespace RestoRate.Contracts.Review.Events;
/// <summary>
/// Интеграционное событие: отзыв отклонён (Rejected).
/// </summary>
/// <param name="ReviewId">Идентификатор отзыва.</param>
public sealed record ReviewRejectedEvent(Guid ReviewId) : IIntegrationEvent;