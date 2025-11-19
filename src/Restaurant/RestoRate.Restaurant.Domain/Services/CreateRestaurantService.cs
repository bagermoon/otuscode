using Ardalis.Result;
using Ardalis.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;
using RestoRate.Restaurant.Domain.Interfaces;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;
using RestaurantEntity = RestoRate.Restaurant.Domain.RestaurantAggregate.Restaurant;

namespace RestoRate.Restaurant.Domain.Services;

public class CreateRestaurantService(
    IRepository<RestaurantEntity> repository,
    IMediator mediator,
    ILogger<CreateRestaurantService> logger) : ICreateRestaurantService
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
        logger.LogInformation("Создание ресторана {RestaurantName}", name);

        var restaurant = new RestaurantAggregate.Restaurant(
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

        logger.LogInformation("Ресторан {RestaurantName} создан с ID {RestaurantId}", name, restaurant.Id);

        return Result<int>.Success(restaurant.Id);
    }
}
