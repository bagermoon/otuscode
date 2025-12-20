using MassTransit;

using RestoRate.BuildingBlocks.Messaging.Identity;

namespace RestoRate.BuildingBlocks.Messaging;

public class ConsumeUserContextFilter<T> : IFilter<ConsumeContext<T>>
where T : class
{
    public void Probe(ProbeContext context) =>
        context.CreateFilterScope(nameof(ConsumeUserContextFilter<T>));

    public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
    {
        if (UserContextHeaderCodec.TryRead(context.Headers, out var userContext))
        {
            context.GetOrAddPayload(() => userContext);
        }

        await next.Send(context);
    }
}
