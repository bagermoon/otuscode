using Microsoft.AspNetCore.Mvc;

using RestoRate.Abstractions.Identity;
using RestoRate.Auth.Authorization;
using RestoRate.Auth.OpenApi;
using RestoRate.RestaurantService.Api.Endpoints.Restaurants;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenApi(opts => opts.AddDocumentTransformer<KeycloakScalarSecurityTransformer>());
builder.AddRestaurantApi();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.MapDefaultEndpoints();

app.MapRestaurantsEndpoints("restaurants")
    //.RequireAuthorization()
    .WithTags("Restaurants");

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
}

app.Run();

public partial class Program { }
