using RestoRate.Abstractions.Messaging;

namespace RestoRate.Contracts.Restaurant.Events;

/// <summary>
/// Интеграционное событие: ресторан архивирован.
/// Используется другими сервисами для обновления локальных проекций и запрета операций по архивированным ресторанам.
/// </summary>
/// <param name="RestaurantId">Идентификатор ресторана.</param>
/// <param name="Status">Текущий статус ресторана после архивирования.</param>
public sealed record RestaurantArchivedEvent(
    Guid RestaurantId,
    RestaurantStatus Status
) : IIntegrationEvent;
