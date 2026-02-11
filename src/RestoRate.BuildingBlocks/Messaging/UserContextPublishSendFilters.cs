using MassTransit;

using RestoRate.Abstractions.Identity;
using RestoRate.BuildingBlocks.Messaging.Identity;

namespace RestoRate.BuildingBlocks.Messaging;

public sealed class PublishUserContextFilter<T>(IUserContext userContext) : IFilter<PublishContext<T>>
    where T : class
{
    private readonly IUserContext _userContext = userContext;

    public void Probe(ProbeContext context) =>
        context.CreateFilterScope(nameof(PublishUserContextFilter<T>));

    public Task Send(PublishContext<T> context, IPipe<PublishContext<T>> next)
    {
        UserContextHeaderCodec.TryWrite(context.Headers, _userContext, overwriteExisting: false);
        return next.Send(context);
    }
}

public sealed class SendUserContextFilter<T>(IUserContext userContext) : IFilter<SendContext<T>>
    where T : class
{
    private readonly IUserContext _userContext = userContext;

    public void Probe(ProbeContext context) =>
        context.CreateFilterScope(nameof(SendUserContextFilter<T>));

    public Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
    {
        UserContextHeaderCodec.TryWrite(context.Headers, _userContext, overwriteExisting: false);
        return next.Send(context);
    }
}
