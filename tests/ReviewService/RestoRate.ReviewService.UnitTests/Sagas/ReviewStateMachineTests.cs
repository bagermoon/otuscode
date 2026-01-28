using FluentAssertions;

using MassTransit;
using MassTransit.Testing;

using Mediator;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using RestoRate.Contracts.Review.Events;
using RestoRate.ReviewService.Application.Sagas.ReviewSaga;
using RestoRate.ReviewService.Application.UseCases.Reviews.MoveToModerationPending;
using RestoRate.ReviewService.Application.UseCases.Reviews.Reject;
using RestoRate.Testing.Common.Helpers;

namespace RestoRate.ReviewService.UnitTests.Sagas;

public sealed class ReviewStateMachineTests(ITestContextAccessor testContextAccessor)
{
    private CancellationToken CancellationToken => testContextAccessor.Current.CancellationToken;
    [Fact]
    public async Task WhenBothValidationsOk_SendsMoveToModerationPendingAndTransitionsToValidationOk()
    {
        var sender = Substitute.For<ISender>();

        var reviewId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var services = new ServiceCollection();
        services.AddSingleton(sender);

        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddSagaStateMachine<ReviewStateMachine, ReviewState>()
                .InMemoryRepository();
        });

        await using var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        var sagaHarness = provider.GetRequiredService<ISagaStateMachineTestHarness<ReviewStateMachine, ReviewState>>();

        await harness.Start();
        try
        {
            await harness.Bus.Publish(new ReviewValidationRequested(
                RestaurantId: restaurantId,
                ReviewId: reviewId,
                UserId: userId), CancellationToken);

            await Eventually.SucceedsAsync(async () =>
            {
                (await sagaHarness.Exists(reviewId, x => x.Validating)).Should().NotBeNull();
            });

            await harness.Bus.Publish(new RestaurantValidationCompleted(
                RestaurantId: restaurantId,
                ReviewId: reviewId,
                ValidatedAt: DateTime.UtcNow,
                IsValid: true), CancellationToken);

            await harness.Bus.Publish(new UserValidationCompleted(
                UserId: userId,
                ReviewId: reviewId,
                ValidatedAt: DateTime.UtcNow,
                IsValid: true), CancellationToken);

            await Eventually.SucceedsAsync(() => sender.Received(1).Send(
                    Arg.Is<MoveReviewToModerationPendingCommand>(x => x.ReviewId == reviewId),
                    Arg.Any<CancellationToken>())
                .AsTask());
        }
        finally
        {
            await harness.Stop(CancellationToken);
        }
    }

    [Fact]
    public async Task WhenOnlyOneValidationCompleted_DoesNotSendAnyCommandAndStaysValidating()
    {
        var sender = Substitute.For<ISender>();

        var reviewId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var services = new ServiceCollection();
        services.AddSingleton(sender);

        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddSagaStateMachine<ReviewStateMachine, ReviewState>()
                .InMemoryRepository();
        });

        await using var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        var sagaHarness = provider.GetRequiredService<ISagaStateMachineTestHarness<ReviewStateMachine, ReviewState>>();

        await harness.Start();
        try
        {
            await harness.Bus.Publish(new ReviewValidationRequested(
                RestaurantId: restaurantId,
                ReviewId: reviewId,
                UserId: userId), CancellationToken);

            await Eventually.SucceedsAsync(async () =>
            {
                (await sagaHarness.Exists(reviewId, x => x.Validating)).Should().NotBeNull();
            });

            await harness.Bus.Publish(new RestaurantValidationCompleted(
                RestaurantId: restaurantId,
                ReviewId: reviewId,
                ValidatedAt: DateTime.UtcNow,
                IsValid: false), CancellationToken);

            await Eventually.SucceedsAsync(async () =>
            {
                (await harness.Consumed.Any<RestaurantValidationCompleted>(CancellationToken)).Should().BeTrue();
            }, timeout: TimeSpan.FromSeconds(1));

            sender.ReceivedCalls().Should().BeEmpty();

            (await sagaHarness.Exists(reviewId, x => x.Validating), TimeSpan.FromMilliseconds(200)).Should().NotBeNull();
            (await sagaHarness.Exists(reviewId, x => x.ValidationOk, TimeSpan.FromMilliseconds(200))).Should().BeNull();
            (await sagaHarness.Exists(reviewId, x => x.ValidationFailed, TimeSpan.FromMilliseconds(200))).Should().BeNull();
        }
        finally
        {
            await harness.Stop(CancellationToken);
        }
    }

    [Fact]
    public async Task WhenAnyValidationFails_SendsRejectAndTransitionsToValidationFailed()
    {
        var sender = Substitute.For<ISender>();

        var reviewId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var services = new ServiceCollection();
        services.AddSingleton(sender);

        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddSagaStateMachine<ReviewStateMachine, ReviewState>()
                .InMemoryRepository();
        });

        await using var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        var sagaHarness = provider.GetRequiredService<ISagaStateMachineTestHarness<ReviewStateMachine, ReviewState>>();

        await harness.Start();
        try
        {
            await harness.Bus.Publish(new ReviewValidationRequested(
                RestaurantId: restaurantId,
                ReviewId: reviewId,
                UserId: userId), CancellationToken);

            await Eventually.SucceedsAsync(async () =>
            {
                (await sagaHarness.Exists(reviewId, x => x.Validating)).Should().NotBeNull();
            });

            await harness.Bus.Publish(new RestaurantValidationCompleted(
                RestaurantId: restaurantId,
                ReviewId: reviewId,
                ValidatedAt: DateTime.UtcNow,
                IsValid: false), CancellationToken);

            await harness.Bus.Publish(new UserValidationCompleted(
                UserId: userId,
                ReviewId: reviewId,
                ValidatedAt: DateTime.UtcNow,
                IsValid: true), CancellationToken);

            await Eventually.SucceedsAsync(() => sender.Received(1).Send(
                    Arg.Is<RejectReviewCommand>(x => x.ReviewId == reviewId),
                    Arg.Any<CancellationToken>())
                .AsTask());
        }
        finally
        {
            await harness.Stop(CancellationToken);
        }
    }
}
