using Ardalis.Result;
using Ardalis.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;
using RestoRate.Restaurant.Domain.Interfaces;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;

namespace RestoRate.Restaurant.Domain.Services;

public class UpdateRestaurantService(
    IRepository<RestaurantAggregate.Restaurant> repository,
    IMediator mediator,
    ILogger<UpdateRestaurantService> logger) : IUpdateRestaurantService
{
    private IUpdateRestaurantService _updateRestaurantServiceImplementation;

    public async Task<Result> UpdateRestaurant(
        int restaurantId,
        string name,
        string description,
        PhoneNumber phoneNumber,
        Email email,
        Location location,
        Money averageCheck,
        RestaurantTag tag)
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
        restaurant.UpdateLocation(location);
        restaurant.UpdateAverageCheck(averageCheck);
        restaurant.UpdateTag(tag);

        await repository.UpdateAsync(restaurant);

        foreach (var domainEvent in restaurant.DomainEvents)
            await mediator.Publish(domainEvent);

        restaurant.ClearDomainEvents();

        logger.LogInformation("Ресторан {RestaurantId} обновлен", restaurantId);

        return Result.Success();
    }
}
