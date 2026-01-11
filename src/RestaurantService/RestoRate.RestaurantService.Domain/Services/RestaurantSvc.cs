using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using Microsoft.Extensions.Logging;

using NodaMoney;

using RestoRate.RestaurantService.Domain.Interfaces;
using RestoRate.RestaurantService.Domain.RestaurantAggregate.Specifications;
using RestoRate.RestaurantService.Domain.TagAggregate;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;

using RestaurantEntity = RestoRate.RestaurantService.Domain.RestaurantAggregate.Restaurant;

namespace RestoRate.RestaurantService.Domain.Services;

public class RestaurantSvc(
    IRepository<RestaurantEntity> repository,
    IMediator mediator,
    ILogger<RestaurantSvc> logger) : IRestaurantService
{
    public async Task<Result<RestaurantEntity>> CreateRestaurantAsync(
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

        var restaurant = RestaurantEntity.Create(
                name,
                description,
                phoneNumber,
                email,
                address,
                location,
                openHours,
                averageCheck
            );

        restaurant
            .AddCuisineTypes(cuisineTypes)
            .AddTags(tags)
            .AddImages(images);

        await repository.AddAsync(restaurant);

        logger.LogCreateRestaurantCompleted(name, restaurant.Id);
        return Result<RestaurantEntity>.Success(restaurant);
    }

    public async Task<Result> UpdateRestaurantAsync(
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
        logger.LogUpdateRestaurantStart(restaurantId);

        var spec = new GetRestaurantByIdSpec(restaurantId);
        var restaurant = await repository.FirstOrDefaultAsync(spec);
        if (restaurant == null)
        {
            logger.LogRestaurantNotFound(restaurantId);
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

        logger.LogUpdateRestaurantCompleted(restaurantId);
        return Result.Success();
    }

    public async Task<Result> DeleteRestaurantAsync(Guid restaurantId)
    {
        logger.LogDeleteRestaurantStart(restaurantId);

        var restaurant = await repository.GetByIdAsync(restaurantId);
        if (restaurant == null)
        {
            logger.LogRestaurantNotFound(restaurantId);
            return Result.NotFound();
        }

        restaurant.MarkDeleted();

        await repository.UpdateAsync(restaurant);

        foreach (var domainEvent in restaurant.DomainEvents)
            await mediator.Publish(domainEvent);

        restaurant.ClearDomainEvents();

        logger.LogDeleteRestaurantCompleted(restaurantId);
        return Result.Success();
    }

    public async Task<Result> AddRestaurantImageAsync(
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

    public async Task<Result> RemoveRestaurantImageAsync(Guid restaurantId, Guid imageId)
    {
        var restaurant = await repository.GetByIdAsync(restaurantId);
        if (restaurant == null)
            return Result.NotFound();

        restaurant.RemoveImage(imageId);
        await repository.UpdateAsync(restaurant);

        return Result.Success();
    }

    public async Task<Result> SetPrimaryImageAsync(Guid restaurantId, Guid imageId)
    {
        var restaurant = await repository.GetByIdAsync(restaurantId);
        if (restaurant == null)
            return Result.NotFound();

        restaurant.SetPrimaryImage(imageId);
        await repository.UpdateAsync(restaurant);

        return Result.Success();
    }
}
