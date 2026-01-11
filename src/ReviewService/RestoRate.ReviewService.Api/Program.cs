using RestoRate.Auth.OpenApi;
using RestoRate.ReviewService.Api.Endpoints.Reviews;
using RestoRate.ServiceDefaults;
using RestoRate.ServiceDefaults.EndpointFilters;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddProblemDetailsDefaults();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(opts => opts.AddDocumentTransformer<KeycloakScalarSecurityTransformer>());

builder.AddReviewApi();

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

// Регистрируем endpoints для отзывов с префиксом "reviews"
api.MapReviewsEndpoints("reviews")
    .RequireAuthorization()
    .WithTags("Reviews");

app.Run();

public partial class Program { }
