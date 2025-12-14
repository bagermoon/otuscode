using MassTransit;

using RestoRate.Abstractions.Identity;
using RestoRate.Abstractions.Messaging;
using RestoRate.BuildingBlocks.Messaging.Identity;

namespace RestoRate.BuildingBlocks.Messaging;

public class MassTransitEventBus(
    IPublishEndpoint publisher,
    IUserContext userContext
) : IIntegrationEventBus
{
    private readonly IPublishEndpoint _publisher = publisher;
    private readonly IUserContext _userContext = userContext;

    Task IIntegrationEventBus.PublishAsync<T>(T @event, CancellationToken ct)
        => _publisher.Publish(@event, ctx =>
        {
            SetAuthHeaders(ctx, overwriteExisting: false);
        }, ct);
    Task IIntegrationEventBus.PublishAsync<T>(T @event, IDictionary<string, object?> headers, CancellationToken ct)
        => _publisher.Publish(@event, ctx =>
        {
            foreach (var kv in headers)
                ctx.Headers.Set(kv.Key, kv.Value);

            SetAuthHeaders(ctx, overwriteExisting: false);
        }, ct);

    private void SetAuthHeaders(PublishContext ctx, bool overwriteExisting)
    {
        UserContextHeaderCodec.TryWrite(ctx.Headers, _userContext, overwriteExisting);
    }
}
