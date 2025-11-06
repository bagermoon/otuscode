using System;

using RestoRate.Shared.Contracts.Abstractions;

namespace RestoRate.Shared.Application.Messaging;

public interface IIntegrationEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class, IIntegrationEvent;
    Task PublishAsync<T>(T @event, IDictionary<string, object?> headers, CancellationToken ct = default) where T : class, IIntegrationEvent;
}
