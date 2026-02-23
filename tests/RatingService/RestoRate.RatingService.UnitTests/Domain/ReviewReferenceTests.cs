using Ardalis.SharedKernel;

using FluentAssertions;

using RestoRate.RatingService.Domain.ReviewReferenceAggregate.Events;

namespace RestoRate.RatingService.UnitTests.Domain;

public sealed class ReviewReferenceTests
{
    [Fact]
    public void Create_Sets_IsApprovedFalse_And_RaisesChangedEvent()
    {
        var reviewId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();

        var review = ReviewReference.Create(
            id: reviewId,
            restaurantId: restaurantId,
            rating: 4.5m,
            averageCheck: null);

        review.IsApproved.Should().BeFalse();
        review.Id.Should().Be(reviewId);
        review.RestaurantId.Should().Be(restaurantId);

        var domainEvents = ((IHasDomainEvents)review).DomainEvents;
        domainEvents.Should().ContainSingle();

        var evt = domainEvents.Single().Should().BeOfType<ReviewReferenceChangedDomainEvent>().Subject;
        evt.ReviewId.Should().Be(reviewId);
        evt.RestaurantId.Should().Be(restaurantId);
        evt.IsApproved.Should().BeFalse();
    }

    [Fact]
    public void Approve_Toggles_IsApprovedTrue_And_IsIdempotent()
    {
        var review = ReviewReference.Create(Guid.NewGuid(), Guid.NewGuid(), 4.0m, averageCheck: null);

        var eventsBefore = ((IHasDomainEvents)review).DomainEvents.Count;

        review.Approve();
        review.IsApproved.Should().BeTrue();

        var eventsAfterApprove = ((IHasDomainEvents)review).DomainEvents.Count;
        eventsAfterApprove.Should().Be(eventsBefore + 1);

        review.Approve();
        review.IsApproved.Should().BeTrue();

        var eventsAfterSecondApprove = ((IHasDomainEvents)review).DomainEvents.Count;
        eventsAfterSecondApprove.Should().Be(eventsAfterApprove);
    }

    [Fact]
    public void Reject_Sets_IsApprovedFalse_And_RaisesChangedEvent()
    {
        var review = ReviewReference.Create(Guid.NewGuid(), Guid.NewGuid(), 4.0m, averageCheck: null);
        review.Approve();
        review.IsApproved.Should().BeTrue();

        var eventsBefore = ((IHasDomainEvents)review).DomainEvents.Count;

        review.Reject();

        review.IsApproved.Should().BeFalse();
        ((IHasDomainEvents)review).DomainEvents.Count.Should().Be(eventsBefore + 1);
    }
}
