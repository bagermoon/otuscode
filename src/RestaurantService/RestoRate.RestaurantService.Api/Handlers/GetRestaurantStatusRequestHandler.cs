using MassTransit;

using Microsoft.EntityFrameworkCore;

using RestoRate.Contracts.Restaurant.Requests;
using RestoRate.Contracts.Restaurant;
using RestoRate.RestaurantService.Infrastructure.Data;
using RestoRate.RestaurantService.Application.Mappings;

using Mediator;
using RestoRate.RestaurantService.Application.UseCases.Restaurants.GetStatus;
using Ardalis.Result;

namespace RestoRate.RestaurantService.Api.Handlers;

public sealed class GetRestaurantStatusRequestHandler(
    ISender sender)
    : IConsumer<GetRestaurantStatusRequest>
{
    public async Task Consume(ConsumeContext<GetRestaurantStatusRequest> context)
    {
        var restaurantId = context.Message.RestaurantId;
        var result = await sender.Send(new GetRestaurantStatusQuery(restaurantId), context.CancellationToken);

        if (result.IsError())
        {
            await context.RespondAsync(new GetRestaurantStatusResponse(
                RestaurantId: restaurantId,
                Exists: false,
                Status: RestaurantStatus.Unknown));
            return;
        }
        var info = result.Value;
        await context.RespondAsync(new GetRestaurantStatusResponse(
            RestaurantId: info.RestaurantId,
            Exists: info.Exists,
            Status: info.Status.ToContract()));
    }
}
