using RestoRate.ServiceDefaults;
using StackExchange.Redis;
using RestoRate.ServiceDefaults.EndpointFilters;
using RestoRate.Auth.OpenApi;
using RestoRate.RatingService.Api;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetailsDefaults();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(opts => opts.AddDocumentTransformer<KeycloakScalarSecurityTransformer>());

builder.AddRatingApi();

builder.AddRedisClient(AppHostProjects.RedisCache, configureOptions: options =>
{
    options.DefaultDatabase = 1;
});

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

api.MapGet("/redis-test", async (IConnectionMultiplexer connectionMux, CancellationToken cancellationToken) =>
{
    await connectionMux.GetDatabase().StringSetAsync("lastForecastGenerated", DateTime.UtcNow.ToString("o"));
    var result = await connectionMux.GetDatabase().StringGetAsync("lastForecastGenerated");

    return result;
})
.WithName("redis-test");

app.Run();

public partial class Program { }