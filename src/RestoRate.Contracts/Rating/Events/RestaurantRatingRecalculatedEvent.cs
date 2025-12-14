using RestoRate.Abstractions.Messaging;
using RestoRate.Contracts.Common.Dtos;

namespace RestoRate.Contracts.Rating.Events;

/// <summary>
/// Интеграционное событие: агрегированные рейтинги ресторана пересчитаны.
/// Содержит снапшот метрик по одобренным и предварительным (ещё не одобренным) отзывам.
/// </summary>
/// <param name="RestaurantId">Идентификатор ресторана.</param>
/// <param name="ApprovedAverageRating">Средний рейтинг по одобренным отзывам.</param>
/// <param name="ApprovedReviewsCount">Количество одобренных отзывов, участвующих в расчёте.</param>
/// <param name="ProvisionalAverageRating">Средний рейтинг по предварительным отзывам.</param>
/// <param name="ProvisionalReviewsCount">Количество предварительных отзывов, участвующих в расчёте.</param>
/// <param name="ApprovedAverageCheck">Средний чек по одобренным отзывам (если доступен).</param>
public sealed record RestaurantRatingRecalculatedEvent(
    Guid RestaurantId,
    decimal ApprovedAverageRating,
    int ApprovedReviewsCount,
    decimal ProvisionalAverageRating,
    int ProvisionalReviewsCount,
    MoneyDto? ApprovedAverageCheck) : IIntegrationEvent;
