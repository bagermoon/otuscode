using Ardalis.Result;
using RestoRate.Shared.SharedKernel.Enums;
using RestoRate.Shared.SharedKernel.ValueObjects;

namespace RestoRate.Restaurant.Domain.Interfaces;

public interface ICreateRestaurantService
{
    Task<Result<int>> CreateRestaurant(
        string name,
        string description,
        PhoneNumber phoneNumber,
        Email email,
        Location location,
        Money averageCheck,
        RestaurantTag tag);
}
