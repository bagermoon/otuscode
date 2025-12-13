using System;

using MassTransit;

using Microsoft.Extensions.DependencyInjection;

using RestoRate.Abstractions.Identity;

namespace RestoRate.BuildingBlocks.Messaging.Identity;

public sealed class MassTransitUserContextProvider : IUserContextProvider
{
    private readonly IServiceProvider _services;

    public MassTransitUserContextProvider(IServiceProvider services) =>
        _services = services ?? throw new ArgumentNullException(nameof(services));

    public int Priority => 100;

    public bool TryGet(out IUserContext userContext)
    {
        userContext = default!;
        var consumeContext = _services.GetRequiredService<ConsumeContext>();

        HeaderUserContext? headerUserContext;
        try
        {
            if (consumeContext.TryGetPayload(out headerUserContext))
            {
                userContext = headerUserContext;
                return true;
            }
        }
        catch (ConsumeContextNotAvailableException)
        {
            return false;
        }

        // for cases when the payload is not available yet
        if (HeaderUserContext.TryGetUserContext(consumeContext, out headerUserContext))
        {
            userContext = headerUserContext;
            return true;
        }

        return false;
    }
}
