using Ardalis.Result;

using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;

namespace RestoRate.Restaurant.Domain.Interfaces;

public interface IUpdateRestaurantService
{
    public Task<Result> UpdateRestaurant(
        int restaurantId,
        string name,
        string description,
        PhoneNumber phoneNumber,
        Email email,
        Address address,
        Location location,
        OpenHours openHours,
        CuisineType cuisineType,
        Money averageCheck,
        RestaurantTag tag);
}
