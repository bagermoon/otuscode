using Mediator;

using RestoRate.Abstractions.Persistence;

namespace RestoRate.RatingService.Application.Behaviors;

internal sealed class UnitOfWorkBehavior<TMessage, TResponse>(
    IUnitOfWork unitOfWork,
    ISessionContext sessionContext)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : notnull, IBaseCommand
{
    public async ValueTask<TResponse> Handle(
        TMessage request,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(request, cancellationToken);

        try
        {
            await sessionContext.BeginTransactionAsync(cancellationToken);

            await unitOfWork.SaveEntitiesAsync(cancellationToken);
            await sessionContext.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await sessionContext.RollbackTransactionAsync(cancellationToken);
            throw;
        }

        await unitOfWork.FlushDomainEventsAsync(cancellationToken);
        return response;
    }
}
