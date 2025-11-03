using Ardalis.Result;

namespace RestoRate.Restaurant.Domain.Interfaces;

public interface IDeleteRestaurantService
{
    public Task<Result> DeleteRestaurant(int  restaurantId);
}
