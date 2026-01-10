using Ardalis.SharedKernel;

using NodaMoney;

namespace RestoRate.RestaurantService.Domain.RestaurantAggregate;

public class RatingSnapshot : EntityBase<Guid>
{
    public Guid RestaurantId { get; private set; }
    public decimal AverageRate { get; private set; }
    public Money AverageCheck { get; private set; }
    public int ReviewCount { get; private set; }
    public decimal ProvisionalAverageRate { get; private set; }
    public Money ProvisionalAverageCheck { get; private set; }
    public int ProvisionalReviewCount { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private RatingSnapshot() { }

    internal RatingSnapshot(Guid restaurantId)
    {
        Id = Guid.NewGuid();
        RestaurantId = restaurantId;
        AverageRate = 0m;
        AverageCheck = Money.Zero;
        ReviewCount = 0;
        ProvisionalAverageRate = 0m;
        ProvisionalAverageCheck = Money.Zero;
        ProvisionalReviewCount = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    internal RatingSnapshot ApplyApproved(decimal averageRate, int reviewCount, Money? averageCheck)
    {
        AverageRate = averageRate;
        ReviewCount = reviewCount;
        AverageCheck = averageCheck ?? Money.Zero;
        UpdatedAt = DateTime.UtcNow;

        return this;
    }

    internal RatingSnapshot ApplyProvisional(decimal averageRate, int reviewCount, Money? averageCheck)
    {
        ProvisionalAverageRate = averageRate;
        ProvisionalReviewCount = reviewCount;
        ProvisionalAverageCheck = averageCheck ?? Money.Zero;
        UpdatedAt = DateTime.UtcNow;

        return this;
    }
}
