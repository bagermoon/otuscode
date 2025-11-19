using Ardalis.Result;
using Ardalis.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;
using RestoRate.Restaurant.Domain.Interfaces;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;
using RestaurantEntity = RestoRate.Restaurant.Domain.RestaurantAggregate.Restaurant;

namespace RestoRate.Restaurant.Domain.Services;

public class RestaurantService(
    IRepository<RestaurantEntity> repository,
    IMediator mediator,
    ILogger<RestaurantService> logger) : IRestaurantService
{
    public async Task<Result<int>> CreateRestaurant(
        string name,
        string description,
        PhoneNumber phoneNumber,
        Email email,
        Address address,
        Location location,
        OpenHours openHours,
        CuisineType cuisineType,
        Money averageCheck,
        RestaurantTag tag)
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
            cuisineType,
            averageCheck,
            tag);

        await repository.AddAsync(restaurant);

        foreach (var domainEvent in restaurant.DomainEvents)
            await mediator.Publish(domainEvent);

        restaurant.ClearDomainEvents();

        logger.LogCreateRestaurantCompleted(name, restaurant.Id);
        return Result<int>.Success(restaurant.Id);
    }

    public async Task<Result> UpdateRestaurant(
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
        RestaurantTag tag)
    {
        logger.LogUpdateRestaurantStart(restaurantId);

        var restaurant = await repository.GetByIdAsync(restaurantId);
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
        restaurant.UpdateCuisineType(cuisineType);
        restaurant.UpdateAverageCheck(averageCheck);
        restaurant.UpdateTag(tag);

        await repository.UpdateAsync(restaurant);

        foreach (var domainEvent in restaurant.DomainEvents)
            await mediator.Publish(domainEvent);

        restaurant.ClearDomainEvents();

        logger.LogUpdateRestaurantCompleted(restaurantId);
        return Result.Success();
    }

    public async Task<Result> DeleteRestaurant(int restaurantId)
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
}
