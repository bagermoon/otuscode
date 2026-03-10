using FluentAssertions;

using Microsoft.Extensions.Logging;

using NodaMoney;

using NSubstitute;

using RestoRate.Abstractions.Messaging;
using RestoRate.Contracts.Rating.Events;
using RestoRate.RatingService.Application.Models;
using RestoRate.RatingService.Application.Services;
using RestoRate.RatingService.Application.UseCases.Rating.Recalculate;
using RestoRate.RatingService.Domain.Interfaces;
using RestoRate.RatingService.Domain.Models;

namespace RestoRate.RatingService.UnitTests.Application;

public sealed class RecalculateRestaurantRatingHandlerTests(ITestContextAccessor testContextAccessor)
{
    private CancellationToken CancellationToken => testContextAccessor.Current.CancellationToken;

    [Fact]
    public async Task Handle_WhenLockCannotBeAcquired_ReturnsSuccessWithoutFurtherWork()
    {
        var restaurantId = Guid.NewGuid();
        var debouncer = Substitute.For<IRatingRecalculationDebouncer>();
        var statsCalculator = Substitute.For<IStatsCalculator>();
        var eventBus = Substitute.For<IIntegrationEventBus>();
        var logger = Substitute.For<ILogger<RecalculateRatingHandler>>();

        debouncer.TryAcquireProcessingLockAsync(restaurantId, Arg.Any<TimeSpan>(), CancellationToken)
            .Returns(false);

        var sut = new RecalculateRatingHandler(debouncer, statsCalculator, eventBus, logger);

        var result = await sut.Handle(new RecalculateRatingCommand(restaurantId), CancellationToken);

        result.IsSuccess.Should().BeTrue();
        await statsCalculator.DidNotReceive().RecalculateLatestAsync(Arg.Any<Guid>(), CancellationToken);
        await eventBus.DidNotReceive().PublishAsync(Arg.Any<RestaurantRatingRecalculatedEvent>(), CancellationToken);
        await debouncer.DidNotReceive().ReleaseProcessingLockAsync(restaurantId, CancellationToken);
    }

    [Fact]
    public async Task Handle_WhenRestaurantIsNotDue_ReturnsSuccessAndReleasesLock()
    {
        var restaurantId = Guid.NewGuid();
        var debouncer = Substitute.For<IRatingRecalculationDebouncer>();
        var statsCalculator = Substitute.For<IStatsCalculator>();
        var eventBus = Substitute.For<IIntegrationEventBus>();
        var logger = Substitute.For<ILogger<RecalculateRatingHandler>>();

        debouncer.TryAcquireProcessingLockAsync(restaurantId, Arg.Any<TimeSpan>(), CancellationToken)
            .Returns(true);
        debouncer.GetDueAtAsync(restaurantId, CancellationToken)
            .Returns(DateTimeOffset.UtcNow.AddSeconds(1));

        var sut = new RecalculateRatingHandler(debouncer, statsCalculator, eventBus, logger);

        var result = await sut.Handle(new RecalculateRatingCommand(restaurantId), CancellationToken);

        result.IsSuccess.Should().BeTrue();
        await statsCalculator.DidNotReceive().RecalculateLatestAsync(Arg.Any<Guid>(), CancellationToken);
        await eventBus.DidNotReceive().PublishAsync(Arg.Any<RestaurantRatingRecalculatedEvent>(), CancellationToken);
        await debouncer.Received(1).ReleaseProcessingLockAsync(restaurantId, CancellationToken);
    }

    [Fact]
    public async Task Handle_WhenRecalculationCompletes_PublishesIntegrationEvent()
    {
        var restaurantId = Guid.NewGuid();
        var dueAt = DateTimeOffset.UtcNow.AddSeconds(-1);
        var debouncer = Substitute.For<IRatingRecalculationDebouncer>();
        var statsCalculator = Substitute.For<IStatsCalculator>();
        var eventBus = Substitute.For<IIntegrationEventBus>();
        var logger = Substitute.For<ILogger<RecalculateRatingHandler>>();

        var recalculated = new RatingRecalculationResult(
            Approved: new RestaurantRatingSnapshot(restaurantId, 4.6m, 8, Money.Zero),
            Provisional: new RestaurantRatingSnapshot(restaurantId, 4.3m, 11, Money.Zero));

        debouncer.TryAcquireProcessingLockAsync(restaurantId, Arg.Any<TimeSpan>(), CancellationToken)
            .Returns(true);
        debouncer.GetDueAtAsync(restaurantId, CancellationToken)
            .Returns(dueAt);
        debouncer.TryCompleteAsync(restaurantId, dueAt, CancellationToken)
            .Returns(true);
        statsCalculator.RecalculateLatestAsync(restaurantId, CancellationToken)
            .Returns(recalculated);

        var sut = new RecalculateRatingHandler(debouncer, statsCalculator, eventBus, logger);

        var result = await sut.Handle(new RecalculateRatingCommand(restaurantId), CancellationToken);

        result.IsSuccess.Should().BeTrue();
        await eventBus.Received(1).PublishAsync(
            Arg.Is<RestaurantRatingRecalculatedEvent>(e =>
                e.RestaurantId == restaurantId &&
                e.ApprovedAverageRating == recalculated.Approved.AverageRating &&
                e.ProvisionalAverageRating == recalculated.Provisional.AverageRating),
            CancellationToken);
        await debouncer.Received(1).ReleaseProcessingLockAsync(restaurantId, CancellationToken);
    }

    [Fact]
    public async Task Handle_WhenCompletionMarkerWasUpdated_DoesNotPublishEvent()
    {
        var restaurantId = Guid.NewGuid();
        var dueAt = DateTimeOffset.UtcNow.AddSeconds(-1);
        var debouncer = Substitute.For<IRatingRecalculationDebouncer>();
        var statsCalculator = Substitute.For<IStatsCalculator>();
        var eventBus = Substitute.For<IIntegrationEventBus>();
        var logger = Substitute.For<ILogger<RecalculateRatingHandler>>();

        var recalculated = new RatingRecalculationResult(
            Approved: new RestaurantRatingSnapshot(restaurantId, 4.6m, 8, Money.Zero),
            Provisional: new RestaurantRatingSnapshot(restaurantId, 4.3m, 11, Money.Zero));

        debouncer.TryAcquireProcessingLockAsync(restaurantId, Arg.Any<TimeSpan>(), CancellationToken)
            .Returns(true);
        debouncer.GetDueAtAsync(restaurantId, CancellationToken)
            .Returns(dueAt);
        debouncer.TryCompleteAsync(restaurantId, dueAt, CancellationToken)
            .Returns(false);
        statsCalculator.RecalculateLatestAsync(restaurantId, CancellationToken)
            .Returns(recalculated);

        var sut = new RecalculateRatingHandler(debouncer, statsCalculator, eventBus, logger);

        var result = await sut.Handle(new RecalculateRatingCommand(restaurantId), CancellationToken);

        result.IsSuccess.Should().BeTrue();
        await eventBus.DidNotReceive().PublishAsync(Arg.Any<RestaurantRatingRecalculatedEvent>(), CancellationToken);
        await debouncer.Received(1).ReleaseProcessingLockAsync(restaurantId, CancellationToken);
    }

    [Fact]
    public async Task Handle_WhenRecalculationThrows_ReleasesLockAndReturnsError()
    {
        var restaurantId = Guid.NewGuid();
        var dueAt = DateTimeOffset.UtcNow.AddSeconds(-1);
        var debouncer = Substitute.For<IRatingRecalculationDebouncer>();
        var statsCalculator = Substitute.For<IStatsCalculator>();
        var eventBus = Substitute.For<IIntegrationEventBus>();
        var logger = Substitute.For<ILogger<RecalculateRatingHandler>>();

        debouncer.TryAcquireProcessingLockAsync(restaurantId, Arg.Any<TimeSpan>(), CancellationToken)
            .Returns(true);
        debouncer.GetDueAtAsync(restaurantId, CancellationToken)
            .Returns(dueAt);
        statsCalculator.RecalculateLatestAsync(restaurantId, CancellationToken)
            .Returns<Task<RatingRecalculationResult>>(_ => throw new InvalidOperationException("boom"));

        var sut = new RecalculateRatingHandler(debouncer, statsCalculator, eventBus, logger);

        var result = await sut.Handle(new RecalculateRatingCommand(restaurantId), CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(error => error.Contains(restaurantId.ToString(), StringComparison.Ordinal));
        await eventBus.DidNotReceive().PublishAsync(Arg.Any<RestaurantRatingRecalculatedEvent>(), CancellationToken);
        await debouncer.Received(1).ReleaseProcessingLockAsync(restaurantId, CancellationToken);
    }
}
