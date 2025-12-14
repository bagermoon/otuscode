using Ardalis.Result;

using Mediator;

using RestoRate.RestaurantService.Application.DTOs;
using RestoRate.RestaurantService.Application.UseCases.GetById;

namespace RestoRate.RestaurantService.Api.Endpoints.Restaurants;

internal static class GetRestaurantByIdEndpoint
{
    public static RouteGroupBuilder MapGetRestaurantById(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:Guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetRestaurantByIdQuery(id), ct);
            return result.Status switch
            {
                ResultStatus.Ok => Results.Ok(result.Value),
                ResultStatus.NotFound => Results.NotFound(),
                ResultStatus.Invalid => Results.BadRequest(result.Errors),
                _ => Results.Problem(string.Join(";", result.Errors))
            };
        })
        .WithName("GetRestaurantById")
        .WithSummary("Получить ресторан по ID")
        .Produces<RestaurantDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status400BadRequest);

        return group;
    }
}
