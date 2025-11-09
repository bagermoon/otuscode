using Ardalis.Result;
using Ardalis.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;
using RestoRate.Restaurant.Domain.Interfaces;
using RestaurantEntity = RestoRate.Restaurant.Domain.RestaurantAggregate.Restaurant;

namespace RestoRate.Restaurant.Domain.Services;

public class DeleteRestaurantService(
    IRepository<RestaurantEntity> repository,
    IMediator mediator,
    ILogger<DeleteRestaurantService> logger) : IDeleteRestaurantService
{
    public async Task<Result> DeleteRestaurant(int restaurantId)
    {
        logger.LogInformation("Удаление ресторана {RestaurantId}", restaurantId);

        var restaurant = await repository.GetByIdAsync(restaurantId);
        if (restaurant == null)
        {
            logger.LogWarning("Ресторан {RestaurantId} не найден", restaurantId);
            return Result.NotFound();
        }

        await repository.DeleteAsync(restaurant);

        foreach (var domainEvent in restaurant.DomainEvents)
            await mediator.Publish(domainEvent);

        restaurant.ClearDomainEvents();

        logger.LogInformation("Ресторан {RestaurantId} удален", restaurantId);

        return Result.Success();
    }
}
