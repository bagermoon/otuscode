namespace RestoRate.RestaurantService.Api.Endpoints.Restaurants;

public static class RegisterRestaurantsEndpoints
{
    public static RouteGroupBuilder MapRestaurantsEndpoints(this RouteGroupBuilder group)
    {
        group.MapCreateRestaurant();
        group.MapUpdateRestaurant();
        group.MapDeleteRestaurant();

        group.MapGetRestaurantById().AllowAnonymous();
        group.MapGetAllRestaurants().AllowAnonymous();

        return group;
    }
}
