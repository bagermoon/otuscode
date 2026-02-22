using RestoRate.ServiceDefaults;
using RestoRate.ServiceDefaults.EndpointFilters;
using RestoRate.Auth.OpenApi;
using RestoRate.Auth.Authorization;
using RestoRate.RatingService.Api;
using RestoRate.RatingService.Application.Services;

using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetailsDefaults();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(opts => opts.AddDocumentTransformer<KeycloakScalarSecurityTransformer>());

builder.AddRatingApi();

// Add services to the container.


var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
    app.UseExceptionHandler();
}
else
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePages();

app.MapDefaultEndpoints();

var api = app.MapGroup("/")
    .AddEndpointFilter<ResultEndpointFilter>();

api.MapGroup("/ratings")
    .WithTags("Ratings")
    .RequireAuthorization(PolicyNames.RequireAdminRole)
    .MapGet("/{restaurantId:guid}", async (
        Guid restaurantId,
        [FromServices] IRatingProviderService ratingProviderService,
        CancellationToken cancellationToken) =>
        await ratingProviderService.GetRatingAsync(restaurantId, cancellationToken))
    .WithName("GetRestaurantRating");

app.Run();

public partial class Program { }