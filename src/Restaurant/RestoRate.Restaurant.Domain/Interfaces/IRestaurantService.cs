using Ardalis.Result;
using RestoRate.Restaurant.Domain.RestaurantAggregate;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;

namespace RestoRate.Restaurant.Domain.Interfaces;

public interface IRestaurantService
{
    Task<Result<Guid>> CreateRestaurant(
        string name,
        string description,
        PhoneNumber phoneNumber,
        Email email,
        Address address,
        Location location,
        OpenHours openHours,
        Money averageCheck,
        IEnumerable<CuisineType> cuisineTypes,
        IEnumerable<Tag> tags,
        IEnumerable<(string Url, string? AltText, bool IsPrimary)>? images = null);

    Task<Result> UpdateRestaurant(
        Guid restaurantId,
        string name,
        string description,
        PhoneNumber phoneNumber,
        Email email,
        Address address,
        Location location,
        OpenHours openHours,
        Money averageCheck,
        IEnumerable<CuisineType> cuisineTypes,
        IEnumerable<Tag> tags);

    Task<Result> DeleteRestaurant(Guid restaurantId);

    Task<Result> AddRestaurantImage(Guid restaurantId, string url, string? altText = null, bool isPrimary = false);
    Task<Result> RemoveRestaurantImage(Guid restaurantId, Guid imageId);
    Task<Result> SetPrimaryImage(Guid restaurantId, Guid imageId);
}
