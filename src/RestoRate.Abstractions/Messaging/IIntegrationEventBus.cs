namespace RestoRate.Abstractions.Messaging;

public interface IIntegrationEventBus
{
    Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class, IIntegrationEvent;
    Task PublishAsync<T>(T @event, IDictionary<string, object?> headers, CancellationToken ct = default) where T : class, IIntegrationEvent;
}
