using Ardalis.SharedKernel;

using RestoRate.ReviewService.Domain.ReviewAggregate;
using RestoRate.SharedKernel.Enums;

namespace RestoRate.ReviewService.Domain.Events;

public sealed class ReviewRejectedDomainEvent(
    Review review,
    ReviewRejectionSource rejectionSource) : DomainEventBase
{
    public Review Review { get; } = review;
    public ReviewRejectionSource RejectionSource { get; } = rejectionSource;
    public ReviewStatus Status => Review.Status;
}
