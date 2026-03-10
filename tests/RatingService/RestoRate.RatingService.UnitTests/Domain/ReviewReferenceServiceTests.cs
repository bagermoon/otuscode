using FluentAssertions;

using MongoDB.Driver;

using NodaMoney;

using NSubstitute;

using RestoRate.RatingService.Domain.Interfaces;
using RestoRate.RatingService.Domain.ReviewReferenceAggregate;
using RestoRate.RatingService.Domain.Services;

namespace RestoRate.RatingService.UnitTests.Domain;

public sealed class ReviewReferenceServiceTests(ITestContextAccessor testContextAccessor)
{
    private CancellationToken CancellationToken => testContextAccessor.Current.CancellationToken;

    [Fact]
    public async Task AddAsync_WhenReviewAlreadyExists_DoesNotInsert()
    {
        var repository = Substitute.For<IReviewReferenceRepository>();
        var reviewId = Guid.NewGuid();
        repository.GetReviewReferenceByIdAsync(reviewId, Arg.Any<CancellationToken>())
            .Returns(new ReviewReference());

        var sut = new ReviewReferenceService(repository);

        await sut.AddAsync(reviewId, Guid.NewGuid(), rating: 4.0m, averageCheck: Money.Zero, CancellationToken);

        await repository.DidNotReceiveWithAnyArgs().AddReviewReferenceAsync(default!, CancellationToken);
    }

    [Fact]
    public async Task AddAsync_WhenReviewDoesNotExist_InsertsNewReference()
    {
        var repository = Substitute.For<IReviewReferenceRepository>();
        var reviewId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();

        repository.GetReviewReferenceByIdAsync(reviewId, Arg.Any<CancellationToken>())
            .Returns((ReviewReference?)null);

        var sut = new ReviewReferenceService(repository);

        await sut.AddAsync(reviewId, restaurantId, rating: 4.2m, averageCheck: null, CancellationToken);

        await repository.Received(1).AddReviewReferenceAsync(
            Arg.Is<ReviewReference>(x => x.Id == reviewId && x.RestaurantId == restaurantId && x.IsApproved == false && x.IsRejected == false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApproveAsync_WhenReviewMissing_InsertsApprovedReference()
    {
        var repository = Substitute.For<IReviewReferenceRepository>();
        var reviewId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();

        repository.GetReviewReferenceByIdAsync(reviewId, Arg.Any<CancellationToken>())
            .Returns((ReviewReference?)null);

        var sut = new ReviewReferenceService(repository);

        await sut.ApproveAsync(reviewId, restaurantId, 4.3m, Money.Zero, CancellationToken);

        await repository.Received(1).AddReviewReferenceAsync(
            Arg.Is<ReviewReference>(x => x.Id == reviewId && x.RestaurantId == restaurantId && x.IsApproved && x.IsRejected == false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApproveAsync_WhenReviewExists_UpdatesApproved()
    {
        var repository = Substitute.For<IReviewReferenceRepository>();
        var existing = ReviewReference.Create(Guid.NewGuid(), Guid.NewGuid(), rating: 4.0m, averageCheck: null);

        repository.GetReviewReferenceByIdAsync(existing.Id, Arg.Any<CancellationToken>())
            .Returns(existing);

        var sut = new ReviewReferenceService(repository);

        await sut.ApproveAsync(existing.Id, existing.RestaurantId, existing.Rating, existing.AverageCheck, CancellationToken);

        existing.IsApproved.Should().BeTrue();
        await repository.Received(1).UpdateReviewReferenceAsync(existing, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RejectAsync_WhenReviewMissing_InsertsRejectedReference()
    {
        var repository = Substitute.For<IReviewReferenceRepository>();
        var reviewId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();

        repository.GetReviewReferenceByIdAsync(reviewId, Arg.Any<CancellationToken>())
            .Returns((ReviewReference?)null);

        var sut = new ReviewReferenceService(repository);

        await sut.RejectAsync(reviewId, restaurantId, 2.7m, null, CancellationToken);

        await repository.Received(1).AddReviewReferenceAsync(
            Arg.Is<ReviewReference>(x => x.Id == reviewId && x.RestaurantId == restaurantId && x.IsRejected && x.IsApproved == false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RejectAsync_WhenReviewExists_UpdatesRejectedState()
    {
        var repository = Substitute.For<IReviewReferenceRepository>();
        var existing = ReviewReference.Create(Guid.NewGuid(), Guid.NewGuid(), rating: 4.0m, averageCheck: null);

        repository.GetReviewReferenceByIdAsync(existing.Id, Arg.Any<CancellationToken>())
            .Returns(existing);

        var sut = new ReviewReferenceService(repository);

        await sut.RejectAsync(existing.Id, existing.RestaurantId, existing.Rating, existing.AverageCheck, CancellationToken);

        existing.IsRejected.Should().BeTrue();
        existing.IsApproved.Should().BeFalse();
        await repository.Received(1).UpdateReviewReferenceAsync(existing, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAsync_WhenReviewAlreadyRejected_DoesNotInsert()
    {
        var repository = Substitute.For<IReviewReferenceRepository>();
        var reviewId = Guid.NewGuid();

        repository.GetReviewReferenceByIdAsync(reviewId, Arg.Any<CancellationToken>())
            .Returns(ReviewReference.CreateRejected(reviewId, Guid.NewGuid(), rating: 1.0m, averageCheck: null));

        var sut = new ReviewReferenceService(repository);

        await sut.AddAsync(reviewId, Guid.NewGuid(), rating: 4.5m, averageCheck: Money.Zero, CancellationToken);

        await repository.DidNotReceiveWithAnyArgs().AddReviewReferenceAsync(default!, CancellationToken);
    }
}
