using RestoRate.Abstractions.Messaging;
using RestoRate.Contracts.Common.Dtos;

namespace RestoRate.Contracts.Review.Events;

/// <summary>
/// Интеграционное событие: отзыв подтверждён (Approved).
/// Используется для финальной фиксации рейтинга в RatingService.
/// </summary>
/// <param name="ReviewId">Идентификатор отзыва.</param>
/// <param name="RestaurantId">Идентификатор ресторана, к которому относится отзыв.</param>
/// <param name="AuthorId">Идентификатор автора отзыва.</param>
/// <param name="Rating">Оценка отзыва (например, 1–5).</param>
/// <param name="AverageCheck">Указанный/рассчитанный чек, связанный с отзывом (если применимо).</param>
/// <param name="Comment">Текстовый комментарий к отзыву (может быть <c>null</c>).</param>
/// <param name="Tags">Теги отзыва (может быть <c>null</c>).</param>
public sealed record ReviewApprovedEvent(
    Guid ReviewId,
    Guid RestaurantId,
    Guid AuthorId,
    decimal Rating,
    MoneyDto? AverageCheck,
    string? Comment,
    string[]? Tags) : IIntegrationEvent;
