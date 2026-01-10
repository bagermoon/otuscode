using Ardalis.Result;

using NodaMoney;

using RestoRate.RestaurantService.Domain.RestaurantAggregate;
using RestoRate.RestaurantService.Domain.TagAggregate;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;

namespace RestoRate.RestaurantService.Domain.Interfaces;

public interface IRestaurantService
{
    Task<Result<Restaurant>> CreateRestaurantAsync(
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

    Task<Result> UpdateRestaurantAsync(
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

    Task<Result> DeleteRestaurantAsync(Guid restaurantId);

    Task<Result> AddRestaurantImageAsync(Guid restaurantId, string url, string? altText = null, bool isPrimary = false);
    Task<Result> RemoveRestaurantImageAsync(Guid restaurantId, Guid imageId);
    Task<Result> SetPrimaryImageAsync(Guid restaurantId, Guid imageId);
}
