using NodaMoney;

using NSubstitute;

using RestoRate.RatingService.Domain.Interfaces;
using RestoRate.RatingService.Domain.Services;

namespace RestoRate.RatingService.UnitTests.Domain;

public sealed class ReviewReferenceServiceTests(ITestContextAccessor testContextAccessor)
{
    private CancellationToken CancellationToken => testContextAccessor.Current.CancellationToken;

    [Fact]
    public async Task AddAsync_DelegatesToAtomicAdd()
    {
        var repository = Substitute.For<IReviewReferenceRepository>();
        var reviewId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();

        repository.TryAddAsync(reviewId, restaurantId, 4.0m, Money.Zero, Arg.Any<CancellationToken>())
            .Returns(false);

        var sut = new ReviewReferenceService(repository);

        await sut.AddAsync(reviewId, restaurantId, rating: 4.0m, averageCheck: Money.Zero, CancellationToken);

        await repository.Received(1).TryAddAsync(reviewId, restaurantId, 4.0m, Money.Zero, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApproveAsync_DelegatesToAtomicApprove()
    {
        var repository = Substitute.For<IReviewReferenceRepository>();
        var reviewId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();

        repository.TryApproveAsync(reviewId, restaurantId, 4.3m, Money.Zero, Arg.Any<CancellationToken>())
            .Returns(true);

        var sut = new ReviewReferenceService(repository);

        await sut.ApproveAsync(reviewId, restaurantId, 4.3m, Money.Zero, CancellationToken);

        await repository.Received(1).TryApproveAsync(reviewId, restaurantId, 4.3m, Money.Zero, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RejectAsync_DelegatesToAtomicReject()
    {
        var repository = Substitute.For<IReviewReferenceRepository>();
        var reviewId = Guid.NewGuid();
        var restaurantId = Guid.NewGuid();

        repository.TryRejectAsync(reviewId, restaurantId, 2.7m, null, Arg.Any<CancellationToken>())
            .Returns(true);

        var sut = new ReviewReferenceService(repository);

        await sut.RejectAsync(reviewId, restaurantId, 2.7m, null, CancellationToken);

        await repository.Received(1).TryRejectAsync(reviewId, restaurantId, 2.7m, null, Arg.Any<CancellationToken>());
    }
}
