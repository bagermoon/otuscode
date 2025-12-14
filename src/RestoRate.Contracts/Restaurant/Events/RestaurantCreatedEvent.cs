using RestoRate.Abstractions.Messaging;

namespace RestoRate.Contracts.Restaurant.Events;

/// <summary>
/// Интеграционное событие: ресторан создан.
/// Используется другими сервисами для построения локальных проекций (reference data) и валидации.
/// </summary>
/// <param name="RestaurantId">Идентификатор ресторана.</param>
/// <param name="Status">Текущий статус ресторана после создания.</param>
public sealed record RestaurantCreatedEvent(
    Guid RestaurantId,
    RestaurantStatus Status
) : IIntegrationEvent;
