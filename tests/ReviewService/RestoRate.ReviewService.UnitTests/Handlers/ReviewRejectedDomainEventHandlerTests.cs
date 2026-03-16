using FluentAssertions;

using NSubstitute;

using RestoRate.Abstractions.Messaging;
using RestoRate.Contracts.Review.Events;
using RestoRate.ReviewService.Application.Handlers;
using RestoRate.ReviewService.Domain.Events;
using RestoRate.ReviewService.Domain.ReviewAggregate;

namespace RestoRate.ReviewService.UnitTests.Handlers;

public sealed class ReviewRejectedDomainEventHandlerTests(ITestContextAccessor testContextAccessor)
{
    private CancellationToken CancellationToken => testContextAccessor.Current.CancellationToken;

    [Fact]
    public async Task Handle_WhenRejectionSourceIsValidation_DoesNotPublishIntegrationEvent()
    {
        var integrationEventBus = Substitute.For<IIntegrationEventBus>();
        var handler = new ReviewRejectedDomainEventHandler(integrationEventBus);
        var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), 3.4m, averageCheck: null, comment: "test");

        var notification = new ReviewRejectedDomainEvent(review, ReviewRejectionSource.Validation);

        await handler.Handle(notification, CancellationToken);

        await integrationEventBus.DidNotReceiveWithAnyArgs()
            .PublishAsync(default(ReviewRejectedEvent)!, CancellationToken);
    }

    [Fact]
    public async Task Handle_WhenRejectionSourceIsModeration_PublishesReviewRejectedEvent()
    {
        var integrationEventBus = Substitute.For<IIntegrationEventBus>();
        var handler = new ReviewRejectedDomainEventHandler(integrationEventBus);
        var restaurantId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var review = Review.Create(restaurantId, authorId, 4.2m, averageCheck: null, comment: "test");

        var notification = new ReviewRejectedDomainEvent(review, ReviewRejectionSource.Moderation);

        await handler.Handle(notification, CancellationToken);

        await integrationEventBus.Received(1).PublishAsync(
            Arg.Is<ReviewRejectedEvent>(x =>
                x.ReviewId == review.Id &&
                x.RestaurantId == restaurantId &&
                x.AuthorId == authorId &&
                x.Rating == review.Rating),
            CancellationToken);
    }
}
