using MassTransit;

using Microsoft.EntityFrameworkCore;

using RestoRate.Contracts.Restaurant.Requests;
using RestoRate.Contracts.Restaurant;
using RestoRate.RestaurantService.Infrastructure.Data;
using RestoRate.RestaurantService.Application.Mappings;

namespace RestoRate.RestaurantService.Api.Handlers;

public sealed class GetRestaurantStatusRequestHandler(
    RestaurantDbContext db)
    : IConsumer<GetRestaurantStatusRequest>
{
    public async Task Consume(ConsumeContext<GetRestaurantStatusRequest> context)
    {
        var restaurantId = context.Message.RestaurantId;
        //  Move to use case
        var restaurant = await db.Restaurants
            .AsNoTracking()
            .Where(r => r.Id == restaurantId)
            .Select(r => new { r.Id, r.RestaurantStatus })
            .FirstOrDefaultAsync(context.CancellationToken);

        if (restaurant is null)
        {
            await context.RespondAsync(new GetRestaurantStatusResponse(
                RestaurantId: restaurantId,
                Exists: false,
                Status: RestaurantStatus.Unknown));
            return;
        }

        await context.RespondAsync(new GetRestaurantStatusResponse(
            RestaurantId: restaurant.Id,
            Exists: true,
            Status: restaurant.RestaurantStatus.ToContract()));
    }
}
