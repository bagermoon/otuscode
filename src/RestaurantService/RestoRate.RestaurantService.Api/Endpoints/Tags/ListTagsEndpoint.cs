using Mediator;

using RestoRate.Contracts.Common.Dtos;
using RestoRate.RestaurantService.Application.UseCases.Tags;

namespace RestoRate.RestaurantService.Api.Endpoints.Tags;

internal static class ListTagsEndpoint
{
    public static RouteHandlerBuilder MapListTags(this RouteGroupBuilder group)
    {
        return group.MapGet("/", async (ISender sender, CancellationToken ct) =>
        {
            var result = await sender.Send(new ListTagsQuery(), ct);

            return Results.Ok(result);
        })
        .WithName("ListTags")
        .AllowAnonymous()
        .WithSummary("Получить список всех тэгов")
        .Produces<List<TagDto>>(StatusCodes.Status200OK);
    }
}
