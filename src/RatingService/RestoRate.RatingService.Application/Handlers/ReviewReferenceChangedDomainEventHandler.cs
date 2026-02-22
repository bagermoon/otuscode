using Ardalis.SharedKernel;

using RestoRate.Abstractions.Messaging;
using RestoRate.RatingService.Application.Mappings;
using RestoRate.RatingService.Application.Services;
using RestoRate.RatingService.Domain.ReviewReferenceAggregate.Events;

namespace RestoRate.RatingService.Application.Handlers;

public sealed class ReviewReferenceChangedDomainEventHandler(
    IStatsCalculator statsCalculator,
    IRatingProviderService ratingProviderService,
    IIntegrationEventBus integrationEventBus)
    : IDomainEventHandler<ReviewReferenceChangedDomainEvent>
{

    public async ValueTask Handle(
        ReviewReferenceChangedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var recalculated = await statsCalculator.RecalculateDebouncedAsync(
            domainEvent.RestaurantId,
            requestedApprovedOnly: domainEvent.IsApproved,
            cancellationToken);

        if (!recalculated)
        {
            return;
        }

        var integrationEvent = await ratingProviderService.GetRatingAsync(domainEvent.RestaurantId, cancellationToken);
        await integrationEventBus.PublishAsync(integrationEvent.ToEvent(), cancellationToken);
    }
}
