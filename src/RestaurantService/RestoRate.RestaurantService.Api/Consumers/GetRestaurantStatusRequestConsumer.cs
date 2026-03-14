using Ardalis.Result;

using MassTransit;

using Mediator;

using RestoRate.Contracts.Restaurant;
using RestoRate.Contracts.Restaurant.Requests;
using RestoRate.RestaurantService.Application.Mappings;
using RestoRate.RestaurantService.Application.UseCases.Restaurants.GetStatus;

namespace RestoRate.RestaurantService.Api.Consumers;

public sealed class GetRestaurantStatusRequestConsumer(
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