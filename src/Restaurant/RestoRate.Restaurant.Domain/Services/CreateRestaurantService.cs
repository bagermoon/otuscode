using Ardalis.Result;
using Ardalis.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;
using RestoRate.Restaurant.Domain.Interfaces;
using RestoRate.Shared.SharedKernel.Enums;
using RestoRate.Shared.SharedKernel.ValueObjects;

namespace RestoRate.Restaurant.Domain.Services;

public class CreateRestaurantService(
    IRepository<RestaurantAggregate.Restaurant> repository,
    IMediator mediator,
    ILogger<CreateRestaurantService> logger) : ICreateRestaurantService
{
    public async Task<Result<int>> CreateRestaurant(
        string name,
        string description,
        PhoneNumber phoneNumber,
        Email email,
        Location location,
        Money averageCheck,
        RestaurantTag tag)
    {
        logger.LogInformation("Создание ресторана {RestaurantName}", name);

        var restaurant = new RestaurantAggregate.Restaurant(
            name,
            description,
            phoneNumber,
            email,
            location,
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
