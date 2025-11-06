using Ardalis.Result;

using RestoRate.Shared.SharedKernel.Enums;
using RestoRate.Shared.SharedKernel.ValueObjects;

namespace RestoRate.Restaurant.Domain.Interfaces;

public interface IUpdateRestaurantService
{
    public Task<Result> UpdateRestaurant(
        int restaurantId,
        string name,
        string description,
        PhoneNumber phoneNumber,
        Email email,
        Location location,
        Money averageCheck,
        RestaurantTag tag);
}
