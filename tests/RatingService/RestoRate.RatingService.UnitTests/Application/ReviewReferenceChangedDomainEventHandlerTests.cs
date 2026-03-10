using FluentAssertions;

using NSubstitute;

using RestoRate.Abstractions.Messaging;
using RestoRate.RatingService.Application.Handlers;
using RestoRate.RatingService.Application.Mappings;
using RestoRate.RatingService.Application.Models;
using RestoRate.RatingService.Application.Services;
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
        var eventBus = Substitute.For<IIntegrationEventBus>();

        statsCalculator.QueueRecalculationAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var sut = new ReviewReferenceChangedDomainEventHandler(statsCalculator);

        await sut.Handle(new ReviewReferenceChangedDomainEvent(review), CancellationToken);

        eventBus.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenFailOpenRecalculated_PublishesRestaurantRatingRecalculatedEvent()
    {
        var restaurantId = Guid.NewGuid();
        var review = ReviewReference.Create(Guid.NewGuid(), restaurantId, 4.0m, averageCheck: null);

        var statsCalculator = Substitute.For<IStatsCalculator>();
        var eventBus = Substitute.For<IIntegrationEventBus>();

        var recalculated = new RatingRecalculationResult(
            Approved: new RestoRate.RatingService.Domain.Models.RestaurantRatingSnapshot(restaurantId, 4.5m, 10, NodaMoney.Money.Zero),
            Provisional: new RestoRate.RatingService.Domain.Models.RestaurantRatingSnapshot(restaurantId, 4.2m, 12, NodaMoney.Money.Zero));

        statsCalculator.QueueRecalculationAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var sut = new ReviewReferenceChangedDomainEventHandler(statsCalculator);

        await sut.Handle(new ReviewReferenceChangedDomainEvent(review), CancellationToken);

        await eventBus.Received(1).PublishAsync(
            Arg.Is<RestoRate.Contracts.Rating.Events.RestaurantRatingRecalculatedEvent>(e =>
                e.RestaurantId == restaurantId &&
                e.ApprovedAverageRating == recalculated.Approved.AverageRating &&
                e.ProvisionalAverageRating == recalculated.Provisional.AverageRating),
            Arg.Any<CancellationToken>());
    }
}
