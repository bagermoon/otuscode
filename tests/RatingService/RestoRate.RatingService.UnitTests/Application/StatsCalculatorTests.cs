using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NodaMoney;

using NSubstitute;

using RestoRate.RatingService.Application.Configurations;
using RestoRate.RatingService.Application.Services;
using RestoRate.RatingService.Domain.Interfaces;
using RestoRate.RatingService.Domain.Models;
using RestoRate.RatingService.Domain.Services;

namespace RestoRate.RatingService.UnitTests.Application;

public sealed class StatsCalculatorTests(ITestContextAccessor testContextAccessor)
{
    private CancellationToken CancellationToken => testContextAccessor.Current.CancellationToken;

    [Fact]
    public async Task QueueRecalculationAsync_WhenMarkChangedSucceeds_ReturnsNull_AndDoesNotRecalculate()
    {
        var restaurantId = Guid.NewGuid();

        var ratingCalculator = Substitute.For<IRatingCalculatorService>();
        var cache = Substitute.For<IRestaurantRatingCache>();
        var debouncer = Substitute.For<IRatingRecalculationDebouncer>();
        var logger = Substitute.For<ILogger<StatsCalculator>>();
        var options = CreateOptionsMonitor(debounceWindowMs: 50);

        var sut = new StatsCalculator(ratingCalculator, cache, debouncer, logger, options);

        await sut.QueueRecalculationAsync(restaurantId, CancellationToken);

        await debouncer.Received(1).MarkChangedAsync(
            restaurantId,
            TimeSpan.FromMilliseconds(50),
            Arg.Any<CancellationToken>());
        await ratingCalculator.DidNotReceiveWithAnyArgs().CalculateAsync(default, default, CancellationToken);
    }

    [Fact]
    public async Task QueueRecalculationAsync_WhenDebouncerThrows_LogsAndDoesNotRecalculate()
    {
        var restaurantId = Guid.NewGuid();

        var ratingCalculator = Substitute.For<IRatingCalculatorService>();
        var cache = Substitute.For<IRestaurantRatingCache>();
        var debouncer = Substitute.For<IRatingRecalculationDebouncer>();
        var logger = Substitute.For<ILogger<StatsCalculator>>();
        var options = CreateOptionsMonitor(debounceWindowMs: 50);

        debouncer.MarkChangedAsync(restaurantId, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("redis down")));

        ratingCalculator.CalculateAsync(restaurantId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new RestaurantRatingSnapshot(restaurantId, 0m, 0, Money.Zero));

        var sut = new StatsCalculator(ratingCalculator, cache, debouncer, logger, options);

        await sut.QueueRecalculationAsync(restaurantId, CancellationToken);

        await ratingCalculator.DidNotReceiveWithAnyArgs().CalculateAsync(default, default, CancellationToken);
    }

    [Fact]
    public async Task RecalculateLatestAsync_RecomputesApprovedAndProvisionalSnapshots()
    {
        var restaurantId = Guid.NewGuid();

        var ratingCalculator = Substitute.For<IRatingCalculatorService>();
        var cache = Substitute.For<IRestaurantRatingCache>();
        var debouncer = Substitute.For<IRatingRecalculationDebouncer>();
        var logger = Substitute.For<ILogger<StatsCalculator>>();
        var options = CreateOptionsMonitor(debounceWindowMs: 50);

        var approved = new RestaurantRatingSnapshot(restaurantId, 4.7m, 2, Money.Zero);
        var provisional = new RestaurantRatingSnapshot(restaurantId, 3.5m, 4, Money.Zero);

        ratingCalculator.CalculateAsync(restaurantId, approvedOnly: true, Arg.Any<CancellationToken>()).Returns(approved);
        ratingCalculator.CalculateAsync(restaurantId, approvedOnly: false, Arg.Any<CancellationToken>()).Returns(provisional);

        var sut = new StatsCalculator(ratingCalculator, cache, debouncer, logger, options);

        var result = await sut.RecalculateLatestAsync(restaurantId, CancellationToken);

        result.Provisional.AverageRating.Should().Be(3.5m);
        result.Approved.AverageRating.Should().Be(4.7m);

        await ratingCalculator.Received(1).CalculateAsync(restaurantId, approvedOnly: true, Arg.Any<CancellationToken>());
        await ratingCalculator.Received(1).CalculateAsync(restaurantId, approvedOnly: false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecalculateAsync_WhenOtherSnapshotCached_DoesNotRecomputeOther()
    {
        var restaurantId = Guid.NewGuid();

        var ratingCalculator = Substitute.For<IRatingCalculatorService>();
        var cache = Substitute.For<IRestaurantRatingCache>();
        var debouncer = Substitute.For<IRatingRecalculationDebouncer>();
        var logger = Substitute.For<ILogger<StatsCalculator>>();
        var options = CreateOptionsMonitor(debounceWindowMs: 50);

        var primary = new RestaurantRatingSnapshot(restaurantId, 4.5m, 10, Money.Zero);
        var other = new RestaurantRatingSnapshot(restaurantId, 4.2m, 8, Money.Zero);

        ratingCalculator.CalculateAsync(restaurantId, approvedOnly: true, Arg.Any<CancellationToken>()).Returns(primary);
        cache.GetAsync(restaurantId, approvedOnly: false, Arg.Any<CancellationToken>()).Returns(other);

        var sut = new StatsCalculator(ratingCalculator, cache, debouncer, logger, options);

        var result = await sut.RecalculateAsync(restaurantId, requestedApprovedOnly: true, CancellationToken);

        result.Approved.AverageRating.Should().Be(4.5m);
        result.Provisional.AverageRating.Should().Be(4.2m);

        await ratingCalculator.Received(1).CalculateAsync(restaurantId, approvedOnly: true, Arg.Any<CancellationToken>());
        await ratingCalculator.DidNotReceive().CalculateAsync(restaurantId, approvedOnly: false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueueRecalculationAsync_WhenDebounceWindowInvalid_UsesDefaultWindow()
    {
        var restaurantId = Guid.NewGuid();

        var ratingCalculator = Substitute.For<IRatingCalculatorService>();
        var cache = Substitute.For<IRestaurantRatingCache>();
        var debouncer = Substitute.For<IRatingRecalculationDebouncer>();
        var logger = Substitute.For<ILogger<StatsCalculator>>();
        var options = CreateOptionsMonitor(debounceWindowMs: 0);

        var sut = new StatsCalculator(ratingCalculator, cache, debouncer, logger, options);

        await sut.QueueRecalculationAsync(restaurantId, CancellationToken);

        await debouncer.Received(1).MarkChangedAsync(
            restaurantId,
            TimeSpan.FromMilliseconds(RatingServiceOptions.DefaultDebounceWindowMs),
            Arg.Any<CancellationToken>());
    }

    private static IOptionsMonitor<RatingServiceOptions> CreateOptionsMonitor(int debounceWindowMs)
    {
        var options = Substitute.For<IOptionsMonitor<RatingServiceOptions>>();
        options.CurrentValue.Returns(new RatingServiceOptions
        {
            DebounceWindowMs = debounceWindowMs,
        });

        return options;
    }
}
