using Ardalis.SharedKernel;

using RestoRate.ReviewService.Domain.ReviewAggregate;
using RestoRate.SharedKernel.Enums;

namespace RestoRate.ReviewService.Domain.Events;

public sealed class ReviewApprovedDomainEvent(Review review) : DomainEventBase
{
    public Review Review { get; } = review;
    public ReviewStatus Status => Review.Status;
}
