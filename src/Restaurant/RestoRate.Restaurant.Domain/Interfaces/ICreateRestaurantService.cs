using Ardalis.Result;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;

namespace RestoRate.Restaurant.Domain.Interfaces;

public interface ICreateRestaurantService
{
    Task<Result<int>> CreateRestaurant(
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
