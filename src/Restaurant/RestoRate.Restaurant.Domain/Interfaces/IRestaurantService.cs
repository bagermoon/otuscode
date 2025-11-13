using Ardalis.Result;

using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;

namespace RestoRate.Restaurant.Domain.Interfaces;

public interface IRestaurantService
{
    Task<Result<int>> CreateRestaurant(
        string name,
        string description,
        PhoneNumber phoneNumber,
        Email email,
        Location location,
        Money averageCheck,
        RestaurantTag tag);

    Task<Result> UpdateRestaurant(
        int restaurantId,
        string name,
        string description,
        PhoneNumber phoneNumber,
        Email email,
        Location location,
        Money averageCheck,
        RestaurantTag tag);

    Task<Result> DeleteRestaurant(int restaurantId);
}
