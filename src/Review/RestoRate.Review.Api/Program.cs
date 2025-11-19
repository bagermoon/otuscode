using RestoRate.Auth.OpenApi;
using RestoRate.Review.Api.Endpoints.Reviews;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(opts => opts.AddDocumentTransformer<KeycloakScalarSecurityTransformer>());

builder.AddReviewApi();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapDefaultEndpoints();

// Регистрируем endpoints для отзывов с префиксом "reviews"
app.MapReviewsEndpoints("reviews")
    .RequireAuthorization()
    .WithTags("Reviews");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
