using RestoRate.Auth.OpenApi;
using RestoRate.RestaurantService.Api.Endpoints.Restaurants;
using RestoRate.RestaurantService.Api.Endpoints.Tags;
using RestoRate.ServiceDefaults;
using RestoRate.ServiceDefaults.EndpointFilters;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetailsDefaults();
builder.Services.AddOpenApi(opts => opts.AddDocumentTransformer<KeycloakScalarSecurityTransformer>());
builder.AddRestaurantApi();

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
api
    .MapGroup("/restaurants")
    .WithTags("Restaurants")
    .RequireAuthorization()
    .MapRestaurantsEndpoints();

api
    .MapGroup("/restaurants/tags")
    .WithTags("Tags")
    .RequireAuthorization()
    .MapListTags();

app.Run();

public partial class Program { }
