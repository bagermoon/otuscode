using RestoRate.ReviewService.Domain.ReviewAggregate;
using RestoRate.SharedKernel.Enums;

namespace RestoRate.ReviewService.UnitTests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var review = Review.Create(
            restaurantId: Guid.NewGuid(),
            userId: Guid.NewGuid(),
            rating: 4.5m,
            averageCheck: null,
            comment: "ok");

        Assert.NotNull(review);
        Assert.Equal(ReviewStatus.Pending, review.Status);
    }
}
