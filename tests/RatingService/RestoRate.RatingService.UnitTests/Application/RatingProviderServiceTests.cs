using FluentAssertions;

using NodaMoney;

using NSubstitute;

using RestoRate.RatingService.Application.Models;
using RestoRate.RatingService.Application.Services;
using RestoRate.RatingService.Domain.Interfaces;
using RestoRate.RatingService.Domain.Models;

namespace RestoRate.RatingService.UnitTests.Application;

public sealed class RatingProviderServiceTests(ITestContextAccessor testContextAccessor)
{
    private CancellationToken CancellationToken => testContextAccessor.Current.CancellationToken;
    [Fact]
    public async Task GetRatingAsync_WhenBothSnapshotsCached_DoesNotRecalculate()
    {
        var restaurantId = Guid.NewGuid();

        var cache = Substitute.For<IRestaurantRatingCache>();
        var statsCalculator = Substitute.For<IStatsCalculator>();

        cache.GetAsync(restaurantId, approvedOnly: true, Arg.Any<CancellationToken>())
            .Returns(new RestaurantRatingSnapshot(restaurantId, 4.5m, 10, new Money(100m, Currency.FromCode("RUB"))));
        cache.GetAsync(restaurantId, approvedOnly: false, Arg.Any<CancellationToken>())
            .Returns(new RestaurantRatingSnapshot(restaurantId, 4.2m, 12, new Money(90m, Currency.FromCode("RUB"))));

        var sut = new RatingProviderService(cache, statsCalculator);

        var result = await sut.GetRatingAsync(restaurantId, CancellationToken);

        result.Approved.RestaurantId.Should().Be(restaurantId);
        result.Provisional.RestaurantId.Should().Be(restaurantId);

        await statsCalculator.DidNotReceiveWithAnyArgs().RecalculateAsync(default, default, CancellationToken);
    }

    [Fact]
    public async Task GetRatingAsync_WhenAnySnapshotMissing_Recalculates()
    {
        var restaurantId = Guid.NewGuid();

        var cache = Substitute.For<IRestaurantRatingCache>();
        var statsCalculator = Substitute.For<IStatsCalculator>();

        cache.GetAsync(restaurantId, approvedOnly: true, Arg.Any<CancellationToken>())
            .Returns((RestaurantRatingSnapshot?)null);
        cache.GetAsync(restaurantId, approvedOnly: false, Arg.Any<CancellationToken>())
            .Returns(new RestaurantRatingSnapshot(restaurantId, 4.2m, 12, Money.Zero));

        var recalculated = new RatingRecalculationResult(
            Approved: new RestaurantRatingSnapshot(restaurantId, 4.6m, 8, Money.Zero),
            Provisional: new RestaurantRatingSnapshot(restaurantId, 4.1m, 9, Money.Zero));

        statsCalculator.RecalculateAsync(restaurantId, requestedApprovedOnly: false, Arg.Any<CancellationToken>())
            .Returns(recalculated);

        var sut = new RatingProviderService(cache, statsCalculator);

        var result = await sut.GetRatingAsync(restaurantId, CancellationToken);

        result.Approved.AverageRating.Should().Be(4.6m);
        result.Provisional.AverageRating.Should().Be(4.1m);

        await statsCalculator.Received(1).RecalculateAsync(restaurantId, requestedApprovedOnly: false, Arg.Any<CancellationToken>());
    }
}
