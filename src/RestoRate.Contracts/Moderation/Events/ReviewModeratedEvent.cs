using RestoRate.Abstractions.Messaging;

namespace RestoRate.Contracts.Moderation.Events;

/// <summary>
/// Интеграционное событие: отзыв прошёл модерацию.
/// Публикуется сервисом Moderation и используется потребителями для изменения статуса отзыва
/// (например, «Одобрен»/«Отклонён») и запуска последующих процессов.
/// </summary>
/// <param name="ReviewId">Идентификатор отзыва.</param>
/// <param name="Approved">Признак одобрения: <c>true</c> — отзыв одобрен, <c>false</c> — отклонён.</param>
/// <param name="Reason">Причина отклонения (может быть <c>null</c> при одобрении).</param>
/// <param name="ModeratorId">Идентификатор модератора (субъекта), принявшего решение.</param>
public sealed record ReviewModeratedEvent(
    Guid ReviewId,
    bool Approved,
    string? Reason,
    Guid ModeratorId) : IIntegrationEvent;
