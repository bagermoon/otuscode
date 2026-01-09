using RestoRate.Abstractions.Messaging;

namespace RestoRate.Contracts.Restaurant.Events;

/// <summary>
/// Интеграционное событие: ресторан обновлен.
/// Используется другими сервисами для обновления локальных проекций и запрета операций по архивированным ресторанам.
/// </summary>
/// <param name="RestaurantId">Идентификатор ресторана.</param>
/// <param name="Status">Текущий статус ресторана после обновления.</param>
public sealed record RestaurantUpdatedEvent(
    Guid RestaurantId,
    RestaurantStatus Status
) : IIntegrationEvent;
