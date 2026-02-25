using FluentAssertions;

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
            Arg.Is<ReviewReference>(x => x.Id == reviewId && x.RestaurantId == restaurantId && x.IsApproved == false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApproveAsync_WhenReviewMissing_NoOps()
    {
        var repository = Substitute.For<IReviewReferenceRepository>();
        repository.GetReviewReferenceByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((ReviewReference?)null);

        var sut = new ReviewReferenceService(repository);

        await sut.ApproveAsync(Guid.NewGuid(), CancellationToken);

        await repository.DidNotReceiveWithAnyArgs().UpdateReviewReferenceAsync(default!, CancellationToken);
    }

    [Fact]
    public async Task ApproveAsync_WhenReviewExists_UpdatesApproved()
    {
        var repository = Substitute.For<IReviewReferenceRepository>();
        var existing = ReviewReference.Create(Guid.NewGuid(), Guid.NewGuid(), rating: 4.0m, averageCheck: null);

        repository.GetReviewReferenceByIdAsync(existing.Id, Arg.Any<CancellationToken>())
            .Returns(existing);

        var sut = new ReviewReferenceService(repository);

        await sut.ApproveAsync(existing.Id, CancellationToken);

        existing.IsApproved.Should().BeTrue();
        await repository.Received(1).UpdateReviewReferenceAsync(existing, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RejectAsync_WhenReviewMissing_NoOps()
    {
        var repository = Substitute.For<IReviewReferenceRepository>();
        repository.GetReviewReferenceByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((ReviewReference?)null);

        var sut = new ReviewReferenceService(repository);

        await sut.RejectAsync(Guid.NewGuid(), CancellationToken);

        await repository.DidNotReceiveWithAnyArgs().DeleteReviewReferenceAsync(default, CancellationToken);
    }

    [Fact]
    public async Task RejectAsync_WhenReviewExists_DeletesReference()
    {
        var repository = Substitute.For<IReviewReferenceRepository>();
        var existing = ReviewReference.Create(Guid.NewGuid(), Guid.NewGuid(), rating: 4.0m, averageCheck: null);

        repository.GetReviewReferenceByIdAsync(existing.Id, Arg.Any<CancellationToken>())
            .Returns(existing);

        var sut = new ReviewReferenceService(repository);

        await sut.RejectAsync(existing.Id, CancellationToken);

        await repository.Received(1).DeleteReviewReferenceAsync(existing.Id, Arg.Any<CancellationToken>());
    }
}
