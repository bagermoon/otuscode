using Ardalis.Result;
using Ardalis.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;
using RestoRate.Restaurant.Domain.Interfaces;
using RestoRate.Restaurant.Domain.TagAggregate;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;
using RestaurantEntity = RestoRate.Restaurant.Domain.RestaurantAggregate.Restaurant;

namespace RestoRate.Restaurant.Domain.Services;

public class RestaurantService(
    IRepository<RestaurantEntity> repository,
    IMediator mediator,
    ILogger<RestaurantService> logger) : IRestaurantService
{
    public async Task<Result<Guid>> CreateRestaurant(
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
        IEnumerable<(string Url, string? AltText, bool IsPrimary)>? images = null)
    {
        logger.LogCreateRestaurantStart(name);

        var restaurant = new RestaurantEntity(
            name,
            description,
            phoneNumber,
            email,
            address,
            location,
            openHours,
            averageCheck);

        foreach (var cuisineType in cuisineTypes)
            restaurant.AddCuisineType(cuisineType);

        foreach (var tag in tags)
            restaurant.AddTag(tag);

        if (images != null)
        {
            int displayOrder = 0;
            foreach (var (url, altText, isPrimary) in images)
                restaurant.AddImage(url, altText, displayOrder++, isPrimary);
        }

        await repository.AddAsync(restaurant);

        logger.LogCreateRestaurantCompleted(name, restaurant.Id);
        return Result<Guid>.Success(restaurant.Id);
    }

    public async Task<Result> UpdateRestaurant(
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
        IEnumerable<Tag> tags)
    {
        logger.LogInformation("Обновление ресторана {RestaurantId}", restaurantId);

        var restaurant = await repository.GetByIdAsync(restaurantId);
        if (restaurant == null)
        {
            logger.LogWarning("Ресторан {RestaurantId} не найден", restaurantId);
            return Result.NotFound();
        }

        restaurant.UpdateName(name);
        restaurant.UpdateDescription(description);
        restaurant.UpdatePhoneNumber(phoneNumber);
        restaurant.UpdateEmail(email);
        restaurant.UpdateAddress(address);
        restaurant.UpdateLocation(location);
        restaurant.UpdateOpenHours(openHours);
        restaurant.UpdateAverageCheck(averageCheck);
        restaurant.UpdateCuisineTypes(cuisineTypes);
        restaurant.UpdateTags(tags);

        await repository.UpdateAsync(restaurant);

        logger.LogInformation("Ресторан {RestaurantId} обновлен", restaurantId);
        return Result.Success();
    }

    public async Task<Result> DeleteRestaurant(Guid restaurantId)
    {
        logger.LogDeleteRestaurantStart(restaurantId);

        var restaurant = await repository.GetByIdAsync(restaurantId);
        if (restaurant == null)
        {
            logger.LogRestaurantNotFound(restaurantId);
            return Result.NotFound();
        }

        restaurant.MarkDeleted();

        await repository.DeleteAsync(restaurant);

        foreach (var domainEvent in restaurant.DomainEvents)
            await mediator.Publish(domainEvent);

        restaurant.ClearDomainEvents();

        logger.LogDeleteRestaurantCompleted(restaurantId);
        return Result.Success();
    }

    public async Task<Result> AddRestaurantImage(
        Guid restaurantId,
        string url,
        string? altText = null,
        bool isPrimary = false)
    {
        var restaurant = await repository.GetByIdAsync(restaurantId);
        if (restaurant == null)
            return Result.NotFound();

        restaurant.AddImage(url, altText, isPrimary: isPrimary);
        await repository.UpdateAsync(restaurant);

        return Result.Success();
    }

    public async Task<Result> RemoveRestaurantImage(Guid restaurantId, Guid imageId)
    {
        var restaurant = await repository.GetByIdAsync(restaurantId);
        if (restaurant == null)
            return Result.NotFound();

        restaurant.RemoveImage(imageId);
        await repository.UpdateAsync(restaurant);

        return Result.Success();
    }

    public async Task<Result> SetPrimaryImage(Guid restaurantId, Guid imageId)
    {
        var restaurant = await repository.GetByIdAsync(restaurantId);
        if (restaurant == null)
            return Result.NotFound();

        restaurant.SetPrimaryImage(imageId);
        await repository.UpdateAsync(restaurant);

        return Result.Success();
    }
}
