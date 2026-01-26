using Ardalis.SharedKernel;

using RestoRate.ReviewService.Domain.Events;

namespace RestoRate.ReviewService.Application.Handlers;

public sealed class ReviewApprovedDomainEventHandler : IDomainEventHandler<ReviewApprovedDomainEvent>
{
    public ValueTask Handle(ReviewApprovedDomainEvent notification, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;
}
