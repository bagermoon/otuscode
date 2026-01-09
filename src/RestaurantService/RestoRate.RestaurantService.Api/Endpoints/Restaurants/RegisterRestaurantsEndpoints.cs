namespace RestoRate.RestaurantService.Api.Endpoints.Restaurants;

public static class RegisterRestaurantsEndpoints
{
    public static RouteGroupBuilder MapRestaurantsEndpoints(this RouteGroupBuilder group)
    {
        group.MapCreateRestaurant();
        group.MapUpdateRestaurant();
        group.MapDeleteRestaurant();
        group.MapModerateRestaurant();

        group.MapGetRestaurantById();
        group.MapGetAllRestaurants();

        return group;
    }
}
