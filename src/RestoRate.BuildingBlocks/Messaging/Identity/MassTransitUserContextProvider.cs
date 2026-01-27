using System;

using MassTransit;
using MassTransit.DependencyInjection;

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

        var contextProvider = _services.GetService<IScopedConsumeContextProvider>();

        if (contextProvider != null && contextProvider.HasContext)
        {
            var consumeContext = contextProvider.GetContext();
            if (consumeContext.TryGetPayload(out HeaderUserContext? headerUserContext))
            {
                userContext = headerUserContext;
                return true;
            }

            if (UserContextHeaderCodec.TryRead(consumeContext.Headers, out headerUserContext))
            {
                userContext = headerUserContext;
                return true;
            }
        }

        return false;
    }
}
