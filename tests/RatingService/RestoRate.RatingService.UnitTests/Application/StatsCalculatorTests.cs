using FluentAssertions;

using Microsoft.Extensions.Logging;

using NodaMoney;

using NSubstitute;

using RestoRate.RatingService.Application.Services;
using RestoRate.RatingService.Domain.Interfaces;
using RestoRate.RatingService.Domain.Models;
using RestoRate.RatingService.Domain.Services;

namespace RestoRate.RatingService.UnitTests.Application;

public sealed class StatsCalculatorTests(ITestContextAccessor testContextAccessor)
{
    private CancellationToken CancellationToken => testContextAccessor.Current.CancellationToken;

    [Fact]
    public async Task RecalculateDebouncedAsync_WhenDebouncerDenies_ReturnsFalse_AndDoesNotRecalculate()
    {
        var restaurantId = Guid.NewGuid();

        var ratingCalculator = Substitute.For<IRatingCalculatorService>();
        var cache = Substitute.For<IRestaurantRatingCache>();
        var debouncer = Substitute.For<IRatingRecalculationDebouncer>();
        var logger = Substitute.For<ILogger<StatsCalculator>>();

        debouncer.TryEnterWindowAsync(restaurantId, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>()).Returns(false);

        var sut = new StatsCalculator(ratingCalculator, cache, debouncer, logger, TimeSpan.FromMilliseconds(50));

        var result = await sut.RecalculateDebouncedAsync(restaurantId, requestedApprovedOnly: true, CancellationToken);

        result.Should().BeFalse();
        await ratingCalculator.DidNotReceiveWithAnyArgs().CalculateAsync(default, default, CancellationToken);
    }

    [Fact]
    public async Task RecalculateDebouncedAsync_WhenDebouncerThrows_FailOpen_ReturnsTrue_AndRecalculates()
    {
        var restaurantId = Guid.NewGuid();

        var ratingCalculator = Substitute.For<IRatingCalculatorService>();
        var cache = Substitute.For<IRestaurantRatingCache>();
        var debouncer = Substitute.For<IRatingRecalculationDebouncer>();
        var logger = Substitute.For<ILogger<StatsCalculator>>();

        debouncer.TryEnterWindowAsync(restaurantId, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<bool>(new InvalidOperationException("redis down")));

        ratingCalculator.CalculateAsync(restaurantId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new RestaurantRatingSnapshot(restaurantId, 0m, 0, Money.Zero));

        var sut = new StatsCalculator(ratingCalculator, cache, debouncer, logger, TimeSpan.FromMilliseconds(50));

        var result = await sut.RecalculateDebouncedAsync(restaurantId, requestedApprovedOnly: true, CancellationToken);

        result.Should().BeTrue();
        await ratingCalculator.Received(2).CalculateAsync(restaurantId, Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecalculateAsync_WhenOtherSnapshotCached_DoesNotRecomputeOther()
    {
        var restaurantId = Guid.NewGuid();

        var ratingCalculator = Substitute.For<IRatingCalculatorService>();
        var cache = Substitute.For<IRestaurantRatingCache>();
        var debouncer = Substitute.For<IRatingRecalculationDebouncer>();
        var logger = Substitute.For<ILogger<StatsCalculator>>();

        var primary = new RestaurantRatingSnapshot(restaurantId, 4.5m, 10, Money.Zero);
        var other = new RestaurantRatingSnapshot(restaurantId, 4.2m, 8, Money.Zero);

        ratingCalculator.CalculateAsync(restaurantId, approvedOnly: true, Arg.Any<CancellationToken>()).Returns(primary);
        cache.GetAsync(restaurantId, approvedOnly: false, Arg.Any<CancellationToken>()).Returns(other);

        var sut = new StatsCalculator(ratingCalculator, cache, debouncer, logger, TimeSpan.FromMilliseconds(50));

        var result = await sut.RecalculateAsync(restaurantId, requestedApprovedOnly: true, CancellationToken);

        result.Approved.AverageRating.Should().Be(4.5m);
        result.Provisional.AverageRating.Should().Be(4.2m);

        await ratingCalculator.Received(1).CalculateAsync(restaurantId, approvedOnly: true, Arg.Any<CancellationToken>());
        await ratingCalculator.DidNotReceive().CalculateAsync(restaurantId, approvedOnly: false, Arg.Any<CancellationToken>());
    }
}
