namespace RestoRate.RestaurantService.Api.Endpoints.Restaurants;

using RestoRate.Auth.Authorization;

public static class RegisterRestaurantsEndpoints
{
    public static RouteGroupBuilder MapRestaurantsEndpoints(this RouteGroupBuilder group)
    {
        var adminGroup = group
            .MapGroup("/")
            .RequireAuthorization(PolicyNames.RequireAdminRole);

        adminGroup.MapCreateRestaurant();
        adminGroup.MapUpdateRestaurant();
        adminGroup.MapDeleteRestaurant();
        adminGroup.MapModerateRestaurant();

        group.MapGetRestaurantsByOwner();
        group.MapGetRestaurantById();
        group.MapGetAllRestaurants();

        return group;
    }
}
