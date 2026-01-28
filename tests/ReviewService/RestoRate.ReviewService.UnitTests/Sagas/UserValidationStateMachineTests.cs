using Ardalis.SharedKernel;

using FluentAssertions;

using MassTransit;
using MassTransit.Testing;

using Mediator;

using Microsoft.Extensions.DependencyInjection;

using NSubstitute;

using RestoRate.Contracts.Review.Events;
using RestoRate.ReviewService.Application.Sagas.UserValidationSaga;
using RestoRate.ReviewService.Application.UseCases.UserReferences.UserReferenceValidation;
using RestoRate.ReviewService.Domain.ReviewAggregate.Specifications;
using RestoRate.Testing.Common.Helpers;
namespace RestoRate.ReviewService.UnitTests.Sagas;

public sealed class UserValidationStateMachineTests(ITestContextAccessor testContextAccessor)
{
    private CancellationToken CancellationToken => testContextAccessor.Current.CancellationToken;
    [Fact]
    public async Task WhenUserValidated_PublishesCompletedEventForEachPendingReview()
    {
        var sender = Substitute.For<ISender>();
        var reviewRepository = Substitute.For<IRepository<Review>>();

        var restaurantId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var pendingReview1 = new Review(restaurantId, userId, rating: 5m, averageCheck: null, comment: null);
        var pendingReview2 = new Review(restaurantId, userId, rating: 4m, averageCheck: null, comment: null);

        reviewRepository
            .ListAsync(Arg.Any<GetPendingReviewsByUserSpec>(), Arg.Any<CancellationToken>())
            .Returns([pendingReview1, pendingReview2]);

        var services = new ServiceCollection();
        services.AddSingleton(sender);
        services.AddSingleton(reviewRepository);

        services.AddMassTransitTestHarness(cfg =>
        {
            cfg.AddSagaStateMachine<UserValidationStateMachine, UserValidationState>()
                .InMemoryRepository();
        });

        await using var provider = services.BuildServiceProvider(true);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        try
        {
            await harness.Bus.Publish(new ReviewValidationRequested(
                RestaurantId: restaurantId,
                ReviewId: Guid.NewGuid(),
                UserId: userId), CancellationToken);

            await Eventually.SucceedsAsync(() => sender.Received(1).Send(
                    Arg.Any<UserReferenceValidationCommand>(),
                    Arg.Any<CancellationToken>())
                .AsTask());

            await harness.Bus.Publish(new UserReferenceValidationStatus(
                UserId: userId,
                IsValid: true), CancellationToken);

            (await harness.Published.Any<UserValidationCompleted>(CancellationToken)).Should().BeTrue();

            var published = harness.Published.Select<UserValidationCompleted>(CancellationToken).ToList();
            published.Should().HaveCount(2);
            published.Select(x => x.Context.Message.ReviewId).Should().BeEquivalentTo([pendingReview1.Id, pendingReview2.Id]);
            published.All(x => x.Context.Message.UserId == userId).Should().BeTrue();
            published.All(x => x.Context.Message.IsValid).Should().BeTrue();
        }
        finally
        {
            await harness.Stop(CancellationToken);
        }
    }
}
