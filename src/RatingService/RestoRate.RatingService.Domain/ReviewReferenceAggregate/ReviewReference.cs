using Ardalis.SharedKernel;

using NodaMoney;

namespace RestoRate.RatingService.Domain.ReviewReferenceAggregate;

public class ReviewReference : EntityBase<Guid>, IAggregateRoot
{
    public Guid RestaurantId { get; set; }
    public decimal Rating { get; set; }
    public Money? AverageCheck { get; set; }
    public bool IsApproved { get; set; }
}