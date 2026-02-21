using Ardalis.SharedKernel;

using NodaMoney;

using RestoRate.Abstractions.Messaging;
using RestoRate.Contracts.Common.Dtos;
using RestoRate.Contracts.Rating.Events;
using RestoRate.RatingService.Application.Services;
using RestoRate.RatingService.Domain.ReviewReferenceAggregate.Events;

namespace RestoRate.RatingService.Application.Handlers;

public sealed class ReviewReferenceChangedDomainEventHandler(
    IRatingProviderService ratingProvider,
    IIntegrationEventBus integrationEventBus)
    : IDomainEventHandler<ReviewReferenceChangedDomainEvent>
{
    private static readonly MoneyDto ZeroRub = new(0m, "RUB");

    public async ValueTask Handle(
        ReviewReferenceChangedDomainEvent domainEvent,
        CancellationToken cancellationToken)
    {
        // Refresh cache + get latest snapshot (best-effort cache).
        var snapshot = await ratingProvider.RefreshRatingAsync(domainEvent.RestaurantId, cancellationToken);

        var integrationEvent = new RestaurantRatingRecalculatedEvent(
            RestaurantId: snapshot.RestaurantId,
            ApprovedAverageRating: snapshot.ApprovedAverageRating,
            ApprovedReviewsCount: snapshot.ApprovedReviewsCount,
            ApprovedAverageCheck: ToMoneyDto(snapshot.ApprovedAverageCheck),
            ProvisionalAverageRating: snapshot.ProvisionalAverageRating,
            ProvisionalReviewsCount: snapshot.ProvisionalReviewsCount,
            ProvisionalAverageCheck: ToMoneyDto(snapshot.ProvisionalAverageCheck)
        );

        await integrationEventBus.PublishAsync(integrationEvent, cancellationToken);

        static MoneyDto ToMoneyDto(Money? money)
            => money is null
                ? ZeroRub
                : new MoneyDto(money.Value.Amount, money.Value.Currency.Code);
    }
}
