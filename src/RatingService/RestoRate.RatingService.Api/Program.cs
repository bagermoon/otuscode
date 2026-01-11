using RestoRate.ServiceDefaults;
using RestoRate.Auth.Authentication;
using StackExchange.Redis;
using RestoRate.ServiceDefaults.EndpointFilters;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetailsDefaults();

builder.AddJwtAuthentication(AppHostProjects.Keycloak);

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminGroup", policy =>
        policy.RequireRole("admin")); // Checks for a "roles" claim with value "admin"

builder.AddRedisClient(AppHostProjects.RedisCache, configureOptions: options =>
{
    options.DefaultDatabase = 1;
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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
