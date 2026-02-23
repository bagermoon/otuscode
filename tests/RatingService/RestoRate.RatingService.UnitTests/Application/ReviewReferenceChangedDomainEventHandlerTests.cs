using FluentAssertions;

using NSubstitute;

using RestoRate.Abstractions.Messaging;
using RestoRate.RatingService.Application.Handlers;
using RestoRate.RatingService.Application.Models;
using RestoRate.RatingService.Application.Services;
using RestoRate.RatingService.Domain.Models;
using RestoRate.RatingService.Domain.ReviewReferenceAggregate.Events;

namespace RestoRate.RatingService.UnitTests.Application;

public sealed class ReviewReferenceChangedDomainEventHandlerTests(ITestContextAccessor testContextAccessor)
{
    private CancellationToken CancellationToken => testContextAccessor.Current.CancellationToken;

    [Fact]
    public async Task Handle_WhenDebounced_DoesNotPublishIntegrationEvent()
    {
        var restaurantId = Guid.NewGuid();
        var review = ReviewReference.Create(Guid.NewGuid(), restaurantId, 4.0m, averageCheck: null);
        review.Approve();

        var statsCalculator = Substitute.For<IStatsCalculator>();
        var ratingProvider = Substitute.For<IRatingProviderService>();
        var eventBus = Substitute.For<IIntegrationEventBus>();

        statsCalculator.RecalculateDebouncedAsync(restaurantId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var sut = new ReviewReferenceChangedDomainEventHandler(statsCalculator, ratingProvider, eventBus);

        await sut.Handle(new ReviewReferenceChangedDomainEvent(review), CancellationToken);

        eventBus.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRecalculated_PublishesRestaurantRatingRecalculatedEvent()
    {
        var restaurantId = Guid.NewGuid();
        var review = ReviewReference.Create(Guid.NewGuid(), restaurantId, 4.0m, averageCheck: null);

        var statsCalculator = Substitute.For<IStatsCalculator>();
        var ratingProvider = Substitute.For<IRatingProviderService>();
        var eventBus = Substitute.For<IIntegrationEventBus>();

        statsCalculator.RecalculateDebouncedAsync(restaurantId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(true);

        ratingProvider.GetRatingAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(new RatingRecalculationResult(
                Approved: new RestaurantRatingSnapshot(restaurantId, 4.5m, 10, NodaMoney.Money.Zero),
                Provisional: new RestaurantRatingSnapshot(restaurantId, 4.2m, 12, NodaMoney.Money.Zero)));

        var sut = new ReviewReferenceChangedDomainEventHandler(statsCalculator, ratingProvider, eventBus);

        await sut.Handle(new ReviewReferenceChangedDomainEvent(review), CancellationToken);

        await eventBus.Received(1).PublishAsync(
            Arg.Is<RestoRate.Contracts.Rating.Events.RestaurantRatingRecalculatedEvent>(e => e.RestaurantId == restaurantId),
            Arg.Any<CancellationToken>());
    }
}
