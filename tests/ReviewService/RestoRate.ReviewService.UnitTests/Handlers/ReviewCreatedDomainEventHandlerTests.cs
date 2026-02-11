using FluentAssertions;

using MassTransit;
using MassTransit.Testing;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NSubstitute;

using RestoRate.Contracts.Review.Events;
using RestoRate.ReviewService.Application.Handlers;
using RestoRate.ReviewService.Domain.Events;

namespace RestoRate.ReviewService.UnitTests.Handlers;

public sealed class ReviewCreatedDomainEventHandlerTests(ITestContextAccessor testContextAccessor)
{
    private CancellationToken CancellationToken => testContextAccessor.Current.CancellationToken;
    [Fact]
    public async Task Handle_PublishesReviewValidationRequested_WithCorrelationId()
    {
        var services = new ServiceCollection();
        services.AddMassTransitTestHarness();

        await using var provider = services.BuildServiceProvider(true);
        await using var scope = provider.CreateAsyncScope();

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        try
        {
            var restaurantId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var review = Review.Create(restaurantId, userId, rating: 4.5m, averageCheck: null, comment: "test");

            var handler = new ReviewCreatedDomainEventHandler(
                scope.ServiceProvider.GetRequiredService<IPublishEndpoint>(),
                Substitute.For<ILogger<ReviewCreatedDomainEventHandler>>());

            await handler.Handle(new ReviewCreatedDomainEvent(review), CancellationToken);

            (await harness.Published.Any<ReviewValidationRequested>(CancellationToken)).Should().BeTrue();

            var published = harness.Published.Select<ReviewValidationRequested>(CancellationToken).First();
            published.Context.CorrelationId.Should().Be(review.Id);
            published.Context.Message.RestaurantId.Should().Be(restaurantId);
            published.Context.Message.UserId.Should().Be(userId);
            published.Context.Message.ReviewId.Should().Be(review.Id);
        }
        finally
        {
            await harness.Stop(CancellationToken);
        }
    }
}
