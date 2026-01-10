using MassTransit;

using Microsoft.Extensions.DependencyInjection;

using RestoRate.Abstractions.Identity;
using RestoRate.Abstractions.Messaging;
using RestoRate.BuildingBlocks.Messaging.Identity;

namespace RestoRate.Testing.Common;

// Test-friendly adapter that resolves IPublishEndpoint per-call to avoid requiring scoped MassTransit
// services during root provider construction in tests.
public class LazyMassTransitEventBus : IIntegrationEventBus
{
    private readonly IServiceProvider _sp;
    private readonly IUserContext _userContext;

    public LazyMassTransitEventBus(IServiceProvider sp, IUserContext userContext)
    {
        _sp = sp;
        _userContext = userContext;
    }

    public Task PublishAsync<T>(T @event, CancellationToken ct = default) where T : class, IIntegrationEvent
    {
        var publisher = _sp.GetRequiredService<IPublishEndpoint>();
        return publisher.Publish(@event, ctx =>
        {
            UserContextHeaderCodec.TryWrite(ctx.Headers, _userContext, overwriteExisting: false);
        }, ct);
    }

    public Task PublishAsync<T>(T @event, IDictionary<string, object?> headers, CancellationToken ct = default) where T : class, IIntegrationEvent
    {
        var publisher = _sp.GetRequiredService<IPublishEndpoint>();
        return publisher.Publish(@event, ctx =>
        {
            foreach (var kv in headers)
                ctx.Headers.Set(kv.Key, kv.Value);

            UserContextHeaderCodec.TryWrite(ctx.Headers, _userContext, overwriteExisting: false);
        }, ct);
    }
}
