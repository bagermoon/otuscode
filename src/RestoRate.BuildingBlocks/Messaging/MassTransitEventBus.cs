using MassTransit;

using RestoRate.Abstractions.Identity;
using RestoRate.Abstractions.Messaging;

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
        if (_userContext is null || !_userContext.IsAuthenticated)
            return;

        void Set(string key, object? value)
        {
            if (!overwriteExisting && ctx.Headers.TryGetHeader(key, out _)) return;
            ctx.Headers.Set(key, value);
        }

        Set(IntegrationHeaders.UserId, _userContext.UserId);
        Set(IntegrationHeaders.UserFullName, _userContext.FullName ?? _userContext.Name);
        Set(IntegrationHeaders.UserName, _userContext.Name);
        Set(IntegrationHeaders.UserEmail, _userContext.Email);
        Set(IntegrationHeaders.UserRoles, _userContext.Roles is null ? null : string.Join(",", _userContext.Roles));
        Set(IntegrationHeaders.IsAuthenticated, _userContext.IsAuthenticated);
    }
}
