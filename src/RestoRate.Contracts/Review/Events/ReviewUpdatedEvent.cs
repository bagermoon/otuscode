using RestoRate.Abstractions.Messaging;
using RestoRate.Contracts.Common.Dtos;

namespace RestoRate.Contracts.Review.Events;

/// <summary>
/// Интеграционное событие: отзыв обновлён.
/// Может отражать изменение содержимого (рейтинг/текст/теги/чек) и/или смену статуса отзыва.
/// </summary>
/// <param name="ReviewId">Идентификатор отзыва.</param>
/// <param name="Status">Текущий статус отзыва после обновления.</param>
/// <param name="Rating">Оценка отзыва (например, 1–5).</param>
/// <param name="AverageCheck">Указанный/рассчитанный чек, связанный с отзывом (если применимо).</param>
/// <param name="Comment">Текстовый комментарий к отзыву (может быть <c>null</c>).</param>
/// <param name="Tags">Теги отзыва (может быть <c>null</c>).</param>
public sealed record ReviewUpdatedEvent(
    Guid ReviewId,
    ReviewStatus Status,
    int Rating,
    MoneyDto? AverageCheck,
    string? Comment,
    string[]? Tags) : IIntegrationEvent;