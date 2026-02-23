using FluentAssertions;

using MassTransit.Testing;

using Microsoft.AspNetCore.Mvc.Testing;

using MongoDB.Driver;

using RestoRate.Contracts.Common.Dtos;
using RestoRate.Contracts.Rating.Events;
using RestoRate.Contracts.Review.Events;
using RestoRate.RatingService.Domain.ReviewReferenceAggregate;
using RestoRate.Testing.Common.Helpers;

namespace RestoRate.RatingService.IntegrationTests.Messaging;

public sealed class ReviewEventsFlowTests : IClassFixture<RatingWebApplicationFactory>, IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestContextAccessor _testContextAccessor;
    private CancellationToken CancellationToken => _testContextAccessor.Current.CancellationToken;
    public ITestHarness Harness { get; private set; } = default!;

    public ReviewEventsFlowTests(RatingWebApplicationFactory factory, ITestContextAccessor testContextAccessor)
    {
        _factory = factory;
        _testContextAccessor = testContextAccessor;
    }

    public async ValueTask InitializeAsync()
    {
        Harness = _factory.Services.GetRequiredService<ITestHarness>();
        await Harness.RestartHostedServices(CancellationToken);
    }
    public async ValueTask DisposeAsync()
    {
        await Harness.Stop(CancellationToken);
    }

    [Fact]
    public async Task ReviewAddedEvent_PersistsReference_And_PublishesRatingRecalculatedEvent()
    {
        var collection = _factory.Services.GetRequiredService<IMongoCollection<ReviewReference>>();

        var restaurantId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();

        var evt = new ReviewAddedEvent(
            ReviewId: reviewId,
            RestaurantId: restaurantId,
            AuthorId: Guid.NewGuid(),
            Rating: 4.0m,
            AverageCheck: new MoneyDto(100m, "USD"),
            Comment: null,
            Tags: null);

        await Harness.Bus.Publish(evt, CancellationToken);

        (await Harness.Consumed.Any<ReviewAddedEvent>(CancellationToken)).Should().BeTrue();

        await Eventually.SucceedsAsync(async () =>
        {
            var saved = await collection.Find(x => x.Id == reviewId).FirstOrDefaultAsync(CancellationToken);
            saved.Should().NotBeNull();
            saved!.RestaurantId.Should().Be(restaurantId);
            saved.IsApproved.Should().BeFalse();
        }, timeout: TimeSpan.FromSeconds(5));

        // Rating event should be published; approved should be zero, provisional should reflect the unapproved review
        (await Harness.Published.Any<RestaurantRatingRecalculatedEvent>(CancellationToken)).Should().BeTrue();

        var published = Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
            .Last(x => x.Context.Message.RestaurantId == restaurantId)
            .Context.Message;
        published.RestaurantId.Should().Be(restaurantId);
        published.ApprovedReviewsCount.Should().Be(0);
        published.ApprovedAverageRating.Should().Be(0m);
        published.ProvisionalReviewsCount.Should().Be(1);
        published.ProvisionalAverageRating.Should().Be(4.0m);

        published.ProvisionalAverageCheck.Currency.Should().Be("USD");
        published.ProvisionalAverageCheck.Amount.Should().Be(100m);
    }

    [Fact]
    public async Task ReviewApprovedEvent_MarksApproved_And_PublishesRatingRecalculatedEvent()
    {
        var collection = _factory.Services.GetRequiredService<IMongoCollection<ReviewReference>>();

        var restaurantId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();

        var added = new ReviewAddedEvent(
            ReviewId: reviewId,
            RestaurantId: restaurantId,
            AuthorId: Guid.NewGuid(),
            Rating: 5.0m,
            AverageCheck: null,
            Comment: null,
            Tags: null);

        var approved = new ReviewApprovedEvent(reviewId);

        await Harness.Bus.Publish(added, CancellationToken);
        (await Harness.Consumed.Any<ReviewAddedEvent>(CancellationToken)).Should().BeTrue();
        // Wait over debounce window before approving so we get a second publish deterministically
        await Task.Delay(250, CancellationToken);

        await Harness.Bus.Publish(approved, CancellationToken);

        await Eventually.SucceedsAsync(async () =>
        {
            Harness.Consumed.Select<ReviewApprovedEvent>(CancellationToken).Any().Should().BeTrue();
        }, timeout: TimeSpan.FromSeconds(5));

        // Poll Mongo until approved
        await Eventually.SucceedsAsync(async () =>
        {
            var saved = await collection.Find(x => x.Id == reviewId).FirstOrDefaultAsync(CancellationToken);
            saved.Should().NotBeNull();
            saved!.IsApproved.Should().BeTrue();
        }, timeout: TimeSpan.FromSeconds(5));

        // Expect at least 2 recalculated events (one for add, one for approve)
        await Eventually.SucceedsAsync(async () =>
        {
            Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken).Count().Should().BeGreaterThanOrEqualTo(2);
        }, timeout: TimeSpan.FromSeconds(5));

        var last = Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
            .Last(x => x.Context.Message.RestaurantId == restaurantId)
            .Context.Message;
        last.RestaurantId.Should().Be(restaurantId);
        last.ApprovedReviewsCount.Should().Be(1);
        last.ApprovedAverageRating.Should().Be(5.0m);
    }

    [Fact]
    public async Task ReviewRejectedEvent_DeletesReference_And_PublishesRatingRecalculatedEvent()
    {
        var collection = _factory.Services.GetRequiredService<IMongoCollection<ReviewReference>>();

        var restaurantId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();

        var added = new ReviewAddedEvent(
            ReviewId: reviewId,
            RestaurantId: restaurantId,
            AuthorId: Guid.NewGuid(),
            Rating: 3.0m,
            AverageCheck: null,
            Comment: null,
            Tags: null);

        var rejected = new ReviewRejectedEvent(reviewId);

        await Harness.Bus.Publish(added, CancellationToken);
        (await Harness.Consumed.Any<ReviewAddedEvent>(CancellationToken)).Should().BeTrue();

        // Wait over debounce window before rejecting so we get a second publish deterministically
        await Task.Delay(350, CancellationToken);

        await Harness.Bus.Publish(rejected, CancellationToken);
        (await Harness.Consumed.Any<ReviewRejectedEvent>(CancellationToken)).Should().BeTrue();

        // Poll Mongo until deleted
        await Eventually.SucceedsAsync(async () =>
        {
            var saved = await collection.Find(x => x.Id == reviewId).FirstOrDefaultAsync(CancellationToken);
            saved.Should().BeNull();
        }, timeout: TimeSpan.FromSeconds(5));

        // Expect at least 2 recalculated events (one for add, one for reject)
        await Eventually.SucceedsAsync(async () =>
        {
            Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken).Count().Should().BeGreaterThanOrEqualTo(2);
        }, timeout: TimeSpan.FromSeconds(5));

        var last = Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
            .Last(x => x.Context.Message.RestaurantId == restaurantId)
            .Context.Message;
        last.RestaurantId.Should().Be(restaurantId);
        last.ProvisionalReviewsCount.Should().Be(0);
        last.ApprovedReviewsCount.Should().Be(0);
    }

    [Fact]
    public async Task DebounceWindow_CoalescesMultipleChangesWithinWindow()
    {
        // use the Harness started in InitializeAsync

        var restaurantId = Guid.NewGuid();

        var first = new ReviewAddedEvent(Guid.NewGuid(), restaurantId, Guid.NewGuid(), 4.0m, null, null, null);
        var second = new ReviewAddedEvent(Guid.NewGuid(), restaurantId, Guid.NewGuid(), 5.0m, null, null, null);

        await Harness.Bus.Publish(first, CancellationToken);
        await Harness.Bus.Publish(second, CancellationToken);

        (await Harness.Consumed.Any<ReviewAddedEvent>(CancellationToken)).Should().BeTrue();

        // Wait until the first recalculation event appears
        await Eventually.SucceedsAsync(async () =>
        {
            var count = Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
                .Count(x => x.Context.Message.RestaurantId == restaurantId);
            count.Should().BeGreaterThanOrEqualTo(1);
        }, timeout: TimeSpan.FromSeconds(5));

        var countWithinWindow = Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
            .Count(x => x.Context.Message.RestaurantId == restaurantId);

        countWithinWindow.Should().Be(1);

        // After window, publishing again should allow another recalculation event
        await Task.Delay(250, CancellationToken);

        var third = new ReviewAddedEvent(Guid.NewGuid(), restaurantId, Guid.NewGuid(), 3.0m, null, null, null);
        await Harness.Bus.Publish(third, CancellationToken);

        await Eventually.SucceedsAsync(async () =>
        {
            var count = Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
                .Count(x => x.Context.Message.RestaurantId == restaurantId);
            count.Should().BeGreaterThanOrEqualTo(2);
        }, timeout: TimeSpan.FromSeconds(5));
    }
}
