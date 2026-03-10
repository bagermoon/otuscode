using FluentAssertions;

using NSubstitute;

using RestoRate.RatingService.Application.Handlers;
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

        statsCalculator.QueueRecalculationAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var sut = new ReviewReferenceChangedDomainEventHandler(statsCalculator);

        await sut.Handle(new ReviewReferenceChangedDomainEvent(review), CancellationToken);

        await statsCalculator.Received(1).QueueRecalculationAsync(restaurantId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReviewChanges_QueuesRecalculationWithoutPublishingIntegrationEvent()
    {
        var restaurantId = Guid.NewGuid();
        var review = ReviewReference.Create(Guid.NewGuid(), restaurantId, 4.0m, averageCheck: null);

        var statsCalculator = Substitute.For<IStatsCalculator>();

        statsCalculator.QueueRecalculationAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var sut = new ReviewReferenceChangedDomainEventHandler(statsCalculator);

        await sut.Handle(new ReviewReferenceChangedDomainEvent(review), CancellationToken);

        await statsCalculator.Received(1).QueueRecalculationAsync(restaurantId, Arg.Any<CancellationToken>());
    }
}
