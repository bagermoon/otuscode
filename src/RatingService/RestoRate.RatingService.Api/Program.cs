using RestoRate.ServiceDefaults;
using RestoRate.Auth.Authentication;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
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
}

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    app.MapOpenApi();
}

app.MapGet("/redis-test", async (IConnectionMultiplexer connectionMux, CancellationToken cancellationToken) =>
{
    await connectionMux.GetDatabase().StringSetAsync("lastForecastGenerated", DateTime.UtcNow.ToString("o"));
    var result = await connectionMux.GetDatabase().StringGetAsync("lastForecastGenerated");

    return result;
})
.WithName("redis-test");

app.Run();
