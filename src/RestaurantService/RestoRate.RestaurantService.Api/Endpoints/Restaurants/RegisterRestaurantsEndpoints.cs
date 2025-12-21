namespace RestoRate.RestaurantService.Api.Endpoints.Restaurants;

public static class RegisterRestaurantsEndpoints
{
    public static RouteGroupBuilder MapRestaurantsEndpoints(this IEndpointRouteBuilder app, string routePrefix = "restaurants")
    {
        var group = app.MapGroup($"/{routePrefix}");

        group.MapCreateRestaurant();
        group.MapGetRestaurantById();
        group.MapGetAllRestaurants();
        group.MapUpdateRestaurant();
        group.MapDeleteRestaurant();

        return group;
    }
}
