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

[Collection(RatingServiceMessagingFlowCollection.Name)]
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

        await Harness.Stop(CancellationToken);
        await Harness.Start();
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

        await Eventually.SucceedsAsync(() =>
        {
            Harness.Consumed.Select<ReviewAddedEvent>(CancellationToken)
                .Any(x => x.Context.Message.ReviewId == reviewId)
                .Should().BeTrue();

            return Task.CompletedTask;
        }, timeout: TimeSpan.FromSeconds(15));

        await Eventually.SucceedsAsync(async () =>
        {
            var saved = await collection.Find(x => x.Id == reviewId).FirstOrDefaultAsync(CancellationToken);
            saved.Should().NotBeNull();
            saved!.RestaurantId.Should().Be(restaurantId);
            saved.IsApproved.Should().BeFalse();
        }, timeout: TimeSpan.FromSeconds(5));

        // Rating event should be published; approved should be zero, provisional should reflect the unapproved review
        await Eventually.SucceedsAsync(() =>
        {
            Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
                .Any(x => x.Context.Message.RestaurantId == restaurantId)
                .Should().BeTrue();

            return Task.CompletedTask;
        }, timeout: TimeSpan.FromSeconds(15));

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
    public async Task DuplicateReviewAddedEvents_CreateSingleReference_And_SingleRecalculation()
    {
        var collection = _factory.Services.GetRequiredService<IMongoCollection<ReviewReference>>();

        var restaurantId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();

        var evt = new ReviewAddedEvent(
            ReviewId: reviewId,
            RestaurantId: restaurantId,
            AuthorId: Guid.NewGuid(),
            Rating: 4.4m,
            AverageCheck: new MoneyDto(150m, "USD"),
            Comment: null,
            Tags: null);

        await Task.WhenAll(
            Harness.Bus.Publish(evt, CancellationToken),
            Harness.Bus.Publish(evt, CancellationToken));

        await Eventually.SucceedsAsync(() =>
        {
            Harness.Consumed.Select<ReviewAddedEvent>(CancellationToken)
                .Count(x => x.Context.Message.ReviewId == reviewId)
                .Should().BeGreaterThanOrEqualTo(2);

            return Task.CompletedTask;
        }, timeout: TimeSpan.FromSeconds(15));

        await Eventually.SucceedsAsync(async () =>
        {
            var saved = await collection.Find(x => x.Id == reviewId).ToListAsync(CancellationToken);
            saved.Should().HaveCount(1);
            saved[0].IsApproved.Should().BeFalse();
            saved[0].IsRejected.Should().BeFalse();
        }, timeout: TimeSpan.FromSeconds(5));

        await Task.Delay(350, CancellationToken);

        Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
            .Count(x => x.Context.Message.RestaurantId == restaurantId)
            .Should().Be(1);
    }

    [Fact]
    public async Task ReviewApprovedEvent_MarksApproved_And_PublishesRatingRecalculatedEvent()
    {
        var collection = _factory.Services.GetRequiredService<IMongoCollection<ReviewReference>>();

        var restaurantId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var added = new ReviewAddedEvent(
            ReviewId: reviewId,
            RestaurantId: restaurantId,
            AuthorId: authorId,
            Rating: 5.0m,
            AverageCheck: null,
            Comment: null,
            Tags: null);

        var approved = new ReviewApprovedEvent(
            ReviewId: reviewId,
            RestaurantId: restaurantId,
            AuthorId: authorId,
            Rating: 5.0m,
            AverageCheck: null,
            Comment: null,
            Tags: null);

        await Harness.Bus.Publish(added, CancellationToken);
        await Eventually.SucceedsAsync(() =>
        {
            Harness.Consumed.Select<ReviewAddedEvent>(CancellationToken)
                .Any(x => x.Context.Message.ReviewId == reviewId)
                .Should().BeTrue();

            return Task.CompletedTask;
        }, timeout: TimeSpan.FromSeconds(15));
        // Wait over debounce window before approving so we get a second publish deterministically
        await Task.Delay(250, CancellationToken);

        await Harness.Bus.Publish(approved, CancellationToken);

        await Eventually.SucceedsAsync(() =>
        {
            Harness.Consumed.Select<ReviewApprovedEvent>(CancellationToken)
                .Any(x => x.Context.Message.ReviewId == reviewId)
                .Should().BeTrue();

            return Task.CompletedTask;
        }, timeout: TimeSpan.FromSeconds(15));

        // Poll Mongo until approved
        await Eventually.SucceedsAsync(async () =>
        {
            var saved = await collection.Find(x => x.Id == reviewId).FirstOrDefaultAsync(CancellationToken);
            saved.Should().NotBeNull();
            saved!.IsApproved.Should().BeTrue();
        }, timeout: TimeSpan.FromSeconds(5));

        // Expect at least 2 recalculated events for this restaurant (one for add, one for approve)
        await Eventually.SucceedsAsync(() =>
        {
            Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
                .Count(x => x.Context.Message.RestaurantId == restaurantId)
                .Should().BeGreaterThanOrEqualTo(2);
            return Task.CompletedTask;
        }, timeout: TimeSpan.FromSeconds(5));

        var last = Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
            .Last(x => x.Context.Message.RestaurantId == restaurantId)
            .Context.Message;
        last.RestaurantId.Should().Be(restaurantId);
        last.ApprovedReviewsCount.Should().Be(1);
        last.ApprovedAverageRating.Should().Be(5.0m);
    }

    [Fact]
    public async Task ReviewAddedThenApproved_WithinSameWindow_PublishesFinalApprovedRating()
    {
        var restaurantId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var added = new ReviewAddedEvent(
            ReviewId: reviewId,
            RestaurantId: restaurantId,
            AuthorId: authorId,
            Rating: 4.6m,
            AverageCheck: null,
            Comment: null,
            Tags: null);

        var approved = new ReviewApprovedEvent(
            ReviewId: reviewId,
            RestaurantId: restaurantId,
            AuthorId: authorId,
            Rating: 4.6m,
            AverageCheck: null,
            Comment: null,
            Tags: null);

        await Harness.Bus.Publish(added, CancellationToken);
        await Task.Delay(25, CancellationToken);
        await Harness.Bus.Publish(approved, CancellationToken);

        await Eventually.SucceedsAsync(() =>
        {
            Harness.Consumed.Select<ReviewAddedEvent>(CancellationToken)
                .Any(x => x.Context.Message.ReviewId == reviewId)
                .Should().BeTrue();

            Harness.Consumed.Select<ReviewApprovedEvent>(CancellationToken)
                .Any(x => x.Context.Message.ReviewId == reviewId)
                .Should().BeTrue();

            return Task.CompletedTask;
        }, timeout: TimeSpan.FromSeconds(15));

        await Eventually.SucceedsAsync(() =>
        {
            var published = Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
                .Where(x => x.Context.Message.RestaurantId == restaurantId)
                .ToList();

            published.Should().NotBeEmpty();

            var last = published.Last().Context.Message;
            last.ApprovedReviewsCount.Should().Be(1);
            last.ApprovedAverageRating.Should().Be(4.6m);
            last.ProvisionalReviewsCount.Should().Be(1);
            last.ProvisionalAverageRating.Should().Be(4.6m);

            return Task.CompletedTask;
        }, timeout: TimeSpan.FromSeconds(15));
    }

    [Fact]
    public async Task ReviewRejectedEvent_DeletesReference_And_PublishesRatingRecalculatedEvent()
    {
        var collection = _factory.Services.GetRequiredService<IMongoCollection<ReviewReference>>();

        var restaurantId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var added = new ReviewAddedEvent(
            ReviewId: reviewId,
            RestaurantId: restaurantId,
            AuthorId: authorId,
            Rating: 3.0m,
            AverageCheck: null,
            Comment: null,
            Tags: null);

        var rejected = new ReviewRejectedEvent(
            ReviewId: reviewId,
            RestaurantId: restaurantId,
            AuthorId: authorId,
            Rating: 3.0m,
            AverageCheck: null,
            Comment: null,
            Tags: null);

        await Harness.Bus.Publish(added, CancellationToken);
        await Eventually.SucceedsAsync(() =>
        {
            Harness.Consumed.Select<ReviewAddedEvent>(CancellationToken)
                .Any(x => x.Context.Message.ReviewId == reviewId)
                .Should().BeTrue();

            return Task.CompletedTask;
        }, timeout: TimeSpan.FromSeconds(15));

        // Wait over debounce window before rejecting so we get a second publish deterministically
        await Task.Delay(350, CancellationToken);

        await Harness.Bus.Publish(rejected, CancellationToken);
        await Eventually.SucceedsAsync(() =>
        {
            Harness.Consumed.Select<ReviewRejectedEvent>(CancellationToken)
                .Any(x => x.Context.Message.ReviewId == reviewId)
                .Should().BeTrue();

            return Task.CompletedTask;
        }, timeout: TimeSpan.FromSeconds(15));

        // Poll Mongo until marked rejected
        await Eventually.SucceedsAsync(async () =>
        {
            var saved = await collection.Find(x => x.Id == reviewId).FirstOrDefaultAsync(CancellationToken);
            saved.Should().NotBeNull();
            saved!.IsRejected.Should().BeTrue();
            saved.IsApproved.Should().BeFalse();
        }, timeout: TimeSpan.FromSeconds(5));

        // Expect at least 2 recalculated events for this restaurant (one for add, one for reject)
        await Eventually.SucceedsAsync(() =>
        {
            Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
                .Count(x => x.Context.Message.RestaurantId == restaurantId)
                .Should().BeGreaterThanOrEqualTo(2);
            return Task.CompletedTask;
        }, timeout: TimeSpan.FromSeconds(5));

        var last = Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
            .Last(x => x.Context.Message.RestaurantId == restaurantId)
            .Context.Message;
        last.RestaurantId.Should().Be(restaurantId);
        last.ProvisionalReviewsCount.Should().Be(0);
        last.ApprovedReviewsCount.Should().Be(0);
    }

    [Fact]
    public async Task ReviewApprovedEvent_BeforeReviewAddedEvent_UpsertsApprovedReference_WithoutDuplicateRecalculation()
    {
        var collection = _factory.Services.GetRequiredService<IMongoCollection<ReviewReference>>();

        var restaurantId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var approved = new ReviewApprovedEvent(
            ReviewId: reviewId,
            RestaurantId: restaurantId,
            AuthorId: authorId,
            Rating: 4.8m,
            AverageCheck: new MoneyDto(80m, "USD"),
            Comment: null,
            Tags: null);

        var added = new ReviewAddedEvent(
            ReviewId: reviewId,
            RestaurantId: restaurantId,
            AuthorId: authorId,
            Rating: 4.8m,
            AverageCheck: new MoneyDto(80m, "USD"),
            Comment: null,
            Tags: null);

        await Harness.Bus.Publish(approved, CancellationToken);

        await Eventually.SucceedsAsync(() =>
        {
            Harness.Consumed.Select<ReviewApprovedEvent>(CancellationToken)
                .Any(x => x.Context.Message.ReviewId == reviewId)
                .Should().BeTrue();

            return Task.CompletedTask;
        }, timeout: TimeSpan.FromSeconds(15));

        await Eventually.SucceedsAsync(async () =>
        {
            var saved = await collection.Find(x => x.Id == reviewId).FirstOrDefaultAsync(CancellationToken);
            saved.Should().NotBeNull();
            saved!.IsApproved.Should().BeTrue();
            saved.IsRejected.Should().BeFalse();
            saved.RestaurantId.Should().Be(restaurantId);
        }, timeout: TimeSpan.FromSeconds(5));

        await Eventually.SucceedsAsync(() =>
        {
            Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
                .Any(x => x.Context.Message.RestaurantId == restaurantId)
                .Should().BeTrue();

            return Task.CompletedTask;
        }, timeout: TimeSpan.FromSeconds(15));

        var publishedCountBeforeAdd = Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
            .Count(x => x.Context.Message.RestaurantId == restaurantId);

        await Harness.Bus.Publish(added, CancellationToken);

        await Eventually.SucceedsAsync(() =>
        {
            Harness.Consumed.Select<ReviewAddedEvent>(CancellationToken)
                .Any(x => x.Context.Message.ReviewId == reviewId)
                .Should().BeTrue();

            return Task.CompletedTask;
        }, timeout: TimeSpan.FromSeconds(15));

        await Task.Delay(350, CancellationToken);

        var savedAfterAdd = await collection.Find(x => x.Id == reviewId).ToListAsync(CancellationToken);
        savedAfterAdd.Should().HaveCount(1);
        savedAfterAdd[0].IsApproved.Should().BeTrue();
        savedAfterAdd[0].IsRejected.Should().BeFalse();

        var publishedCountAfterAdd = Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
            .Count(x => x.Context.Message.RestaurantId == restaurantId);
        publishedCountAfterAdd.Should().Be(publishedCountBeforeAdd);
    }

    [Fact]
    public async Task ReviewRejectedEvent_BeforeReviewAddedEvent_PreservesRejectedTombstone_And_ZeroVisibleRatings()
    {
        var collection = _factory.Services.GetRequiredService<IMongoCollection<ReviewReference>>();

        var restaurantId = Guid.NewGuid();
        var reviewId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var rejected = new ReviewRejectedEvent(
            ReviewId: reviewId,
            RestaurantId: restaurantId,
            AuthorId: authorId,
            Rating: 2.1m,
            AverageCheck: null,
            Comment: null,
            Tags: null);

        var added = new ReviewAddedEvent(
            ReviewId: reviewId,
            RestaurantId: restaurantId,
            AuthorId: authorId,
            Rating: 2.1m,
            AverageCheck: null,
            Comment: null,
            Tags: null);

        await Harness.Bus.Publish(rejected, CancellationToken);

        await Eventually.SucceedsAsync(() =>
        {
            Harness.Consumed.Select<ReviewRejectedEvent>(CancellationToken)
                .Any(x => x.Context.Message.ReviewId == reviewId)
                .Should().BeTrue();

            return Task.CompletedTask;
        }, timeout: TimeSpan.FromSeconds(15));

        await Eventually.SucceedsAsync(async () =>
        {
            var saved = await collection.Find(x => x.Id == reviewId).FirstOrDefaultAsync(CancellationToken);
            saved.Should().NotBeNull();
            saved!.IsRejected.Should().BeTrue();
            saved.IsApproved.Should().BeFalse();
        }, timeout: TimeSpan.FromSeconds(5));

        await Eventually.SucceedsAsync(() =>
        {
            Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
                .Any(x => x.Context.Message.RestaurantId == restaurantId)
                .Should().BeTrue();

            return Task.CompletedTask;
        }, timeout: TimeSpan.FromSeconds(15));

        var publishedCountBeforeAdd = Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
            .Count(x => x.Context.Message.RestaurantId == restaurantId);

        await Harness.Bus.Publish(added, CancellationToken);

        await Eventually.SucceedsAsync(() =>
        {
            Harness.Consumed.Select<ReviewAddedEvent>(CancellationToken)
                .Any(x => x.Context.Message.ReviewId == reviewId)
                .Should().BeTrue();

            return Task.CompletedTask;
        }, timeout: TimeSpan.FromSeconds(15));

        await Task.Delay(350, CancellationToken);

        var savedAfterAdd = await collection.Find(x => x.Id == reviewId).ToListAsync(CancellationToken);
        savedAfterAdd.Should().HaveCount(1);
        savedAfterAdd[0].IsRejected.Should().BeTrue();
        savedAfterAdd[0].IsApproved.Should().BeFalse();

        var publishedCountAfterAdd = Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
            .Count(x => x.Context.Message.RestaurantId == restaurantId);
        publishedCountAfterAdd.Should().Be(publishedCountBeforeAdd);

        var last = Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
            .Last(x => x.Context.Message.RestaurantId == restaurantId)
            .Context.Message;
        last.ApprovedReviewsCount.Should().Be(0);
        last.ProvisionalReviewsCount.Should().Be(0);
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

        await Eventually.SucceedsAsync(() =>
        {
            Harness.Consumed.Select<ReviewAddedEvent>(CancellationToken)
                .Count(x => x.Context.Message.RestaurantId == restaurantId)
                .Should().BeGreaterThanOrEqualTo(2);

            return Task.CompletedTask;
        }, timeout: TimeSpan.FromSeconds(15));

        // Wait until the first recalculation event appears
        await Eventually.SucceedsAsync(() =>
        {
            var count = Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
                .Count(x => x.Context.Message.RestaurantId == restaurantId);
            count.Should().BeGreaterThanOrEqualTo(1);
            return Task.CompletedTask;
        }, timeout: TimeSpan.FromSeconds(5));

        var countWithinWindow = Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
            .Count(x => x.Context.Message.RestaurantId == restaurantId);

        countWithinWindow.Should().Be(1);

        // After window, publishing again should allow another recalculation event
        await Task.Delay(250, CancellationToken);

        var third = new ReviewAddedEvent(Guid.NewGuid(), restaurantId, Guid.NewGuid(), 3.0m, null, null, null);
        await Harness.Bus.Publish(third, CancellationToken);

        await Eventually.SucceedsAsync(() =>
        {
            var count = Harness.Published.Select<RestaurantRatingRecalculatedEvent>(CancellationToken)
                .Count(x => x.Context.Message.RestaurantId == restaurantId);
            count.Should().BeGreaterThanOrEqualTo(2);
            return Task.CompletedTask;
        }, timeout: TimeSpan.FromSeconds(5));
    }
}
