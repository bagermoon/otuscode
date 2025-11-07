
using MassTransit;
using RestoRate.Abstractions.Messaging;

namespace RestoRate.BuildingBlocks.Messaging;

public class MassTransitEventBus(
    IPublishEndpoint publisher
) : IIntegrationEventBus
{
    private readonly IPublishEndpoint _publisher = publisher;

    Task IIntegrationEventBus.PublishAsync<T>(T @event, CancellationToken ct)
        => _publisher.Publish(@event, ct);
    Task IIntegrationEventBus.PublishAsync<T>(T @event, IDictionary<string, object?> headers, CancellationToken ct)
    => _publisher.Publish(@event, ctx =>
        {
            foreach (var kv in headers)
                ctx.Headers.Set(kv.Key, kv.Value);
        }, ct);
}
