using Ardalis.SharedKernel;

using RestoRate.SharedKernel.Enums;

namespace RestoRate.ReviewService.Domain.RestaurantReferenceAggregate;

public class RestaurantReference : EntityBase<Guid>, IAggregateRoot
{
    public RestaurantStatus RestaurantStatus { get; private set; } = RestaurantStatus.Unknown;
    public DateTime? LastSynchronizedAt { get; private set; }

    private RestaurantReference() { }
    public RestaurantReference(Guid id, RestaurantStatus? status = null, DateTime? lastSynchronizedAt = null)
    {
        Id = id;
        RestaurantStatus = status ?? RestaurantStatus.Unknown;
        LastSynchronizedAt = lastSynchronizedAt;
    }

    public static RestaurantReference Create(
        Guid id,
        RestaurantStatus? status = null,
        DateTime? lastSynchronizedAt = null
    )
    {
        var restaurant = new RestaurantReference(id, status, lastSynchronizedAt);

        return restaurant;
    }

    public RestaurantReference Refresh(RestaurantStatus status, DateTime synchronizedAtUtc)
    {
        RestaurantStatus = status;
        LastSynchronizedAt = synchronizedAtUtc;

        return this;
    }
}
