using Ardalis.SharedKernel;

using RestoRate.Abstractions.Messaging;
using RestoRate.RatingService.Application.Mappings;
using RestoRate.RatingService.Application.Services;
using RestoRate.RatingService.Domain.ReviewReferenceAggregate.Events;

namespace RestoRate.RatingService.Application.Handlers;

public sealed class ReviewReferenceChangedDomainEventHandler(
    IStatsCalculator statsCalculator)
    : IDomainEventHandler<ReviewReferenceChangedDomainEvent>
{

    public async ValueTask Handle(
        ReviewReferenceChangedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        await statsCalculator.QueueRecalculationAsync(
            domainEvent.RestaurantId,
            cancellationToken);
    }
}
