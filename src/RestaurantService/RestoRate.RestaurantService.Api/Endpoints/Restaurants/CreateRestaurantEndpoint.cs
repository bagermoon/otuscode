using System.Security.Claims;

using Ardalis.Result;

using Mediator;

using Microsoft.AspNetCore.Mvc;

using RestoRate.Contracts.Restaurant.DTOs;
using RestoRate.Contracts.Restaurant.DTOs.CRUD;
using RestoRate.RestaurantService.Application.UseCases.Restaurants.Create;

namespace RestoRate.RestaurantService.Api.Endpoints.Restaurants;

internal static class CreateRestaurantEndpoint
{
    public static RouteHandlerBuilder MapCreateRestaurant(this RouteGroupBuilder group)
    {
        return group.MapPost("/", async (
            [FromBody] CreateRestaurantDto dto,
            ClaimsPrincipal user,
            [FromServices] ISender sender,
            CancellationToken ct) =>
        {
            var userIdString = user.FindFirst("sub")?.Value
                               ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdString, out var ownerId))
                return Results.Unauthorized();

            var result = await sender.Send(new CreateRestaurantCommand(dto, ownerId), ct);
            if (result.IsSuccess)
                return Results.Created($"/restaurants/{result.Value.RestaurantId}", result.Value);

            return Results.BadRequest(result.Errors);
        })
        .WithName("CreateRestaurant")
        .WithSummary("Создать ресторан")
        .Produces<RestaurantDto>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
