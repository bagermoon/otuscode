using Ardalis.SharedKernel;

using RestoRate.SharedKernel.Enums;

namespace RestoRate.ReviewService.Domain.RestaurantReferenceAggregate;

public class RestaurantReference : EntityBase<Guid>, IAggregateRoot
{
    public RestaurantStatus RestaurantStatus { get; private set; } = RestaurantStatus.Unknown;

    private RestaurantReference() { }
    public RestaurantReference(Guid id, RestaurantStatus? status = null)
    {
        Id = id;
        RestaurantStatus = status ?? RestaurantStatus.Unknown;
    }

    public static RestaurantReference Create(
        Guid id,
        RestaurantStatus? status = null
    )
    {
        var restaurant = new RestaurantReference(id, status);

        return restaurant;
    }
    public RestaurantReference UpdateStatus(RestaurantStatus status)
    {
        RestaurantStatus = status;
        
        return this;
    }
}
