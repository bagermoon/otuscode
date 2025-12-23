using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Projects;
using RestoRate.ServiceDefaults;
using Scalar.Aspire;
using RestoRate.AppHost.Configuration;

// https://fiodar.substack.com/p/a-guide-to-securing-net-aspire-apps
// https://www.youtube.com/watch?v=_aCuwWiKncY
var builder = DistributedApplication.CreateBuilder(args);


var isProduction = builder.Environment.IsProduction();
var appHostConfig = builder.Configuration.GetSection("AppHostConfiguration").Get<AppHostConfiguration>()
    ?? throw new InvalidOperationException("AppHostConfiguration section is missing or invalid in appsettings.json");
var useVolumes = appHostConfig.UseVolumes;
var useDedicatedPorts = appHostConfig.UseDedicatedPorts && !isProduction;

#region Resources
// Postgres resource setup via extension method
var postgres = builder.AddPostgresResource(appHostConfig.Postgres, useVolumes, useDedicatedPorts);
var restaurantDb = postgres.AddDatabase(AppHostProjects.RestaurantDb, "Restaurants");

// Mongo resource setup via extension method
var mongo = builder.AddMongoResource(appHostConfig.Mongo, useVolumes, useDedicatedPorts);

// Redis resource setup via extension method
var redisCache = builder.AddRedisResource(appHostConfig.Redis, useVolumes, useDedicatedPorts);

// Keycloak resource setup via extension method
var keycloakRealm = builder.AddParameter("keycloak-realm", value: "restorate", publishValueAsDefault: true);
var keycloak = builder.AddKeycloakResource(appHostConfig.Keycloak, useVolumes, useDedicatedPorts);

// RabbitMQ resource setup via extension method
var rabbitmq = builder.AddRabbitResource(appHostConfig.Rabbit, useVolumes);

// Scalar resource setup via extension method
var scalar = builder.AddScalarApiReferenceResource(appHostConfig.Scalar)
    .WithReference(keycloak);
#endregion


#region Migrations
var migrations = builder.AddProject<RestoRate_Migrations>("migrations")
    .WithReference(restaurantDb)
    .WaitFor(restaurantDb);
#endregion


#region ServiceRestaurantApi
var restaurantApiBearerAudience = builder.AddParameter("restaurant-api-bearer-audience", value: "restorate-restaurant-api", publishValueAsDefault: true);
var restaurantApi = builder.AddProject<RestoRate_RestaurantService_Api>(AppHostProjects.ServiceRestaurantApi)
    .WithReference(keycloak)
    .WithReference(restaurantDb)
    .WithReference(rabbitmq)
    .WaitFor(keycloak).WaitFor(migrations)
    .WithEnvironment("KeycloakSettings__Audience", restaurantApiBearerAudience)
    .WithEnvironment("KeycloakSettings__Realm", keycloakRealm);

scalar.WithApiReference(restaurantApi, options =>
{
    options.AddDocument("v1", "Restaurant API");
});
#endregion


#region ServiceModerationApi
var moderationApiBearerAudience = builder.AddParameter("moderation-api-bearer-audience", value: "restorate-moderation-api", publishValueAsDefault: true);
var moderationApi = builder.AddProject<RestoRate_ModerationService_Api>(AppHostProjects.ServiceModerationApi)
    .WithReference(keycloak)
    .WithReference(rabbitmq)
    .WaitFor(keycloak).WaitFor(migrations)
    .WithEnvironment("KeycloakSettings__Audience", moderationApiBearerAudience)
    .WithEnvironment("KeycloakSettings__Realm", keycloakRealm);

scalar.WithApiReference(moderationApi, options =>
{
    options.AddDocument("v1", "Moderation API");
});
#endregion


#region ServiceRatingApi
var ratingApiBearerAudience = builder.AddParameter("rating-api-bearer-audience", value: "restorate-rating-api", publishValueAsDefault: true);
var ratingApi = builder.AddProject<RestoRate_RatingService_Api>(AppHostProjects.ServiceRatingApi)
    .WithReference(keycloak)
    .WithReference(rabbitmq)
    .WithReference(redisCache)
    .WaitFor(keycloak).WaitFor(redisCache)
    .WithEnvironment("KeycloakSettings__Audience", ratingApiBearerAudience)
    .WithEnvironment("KeycloakSettings__Realm", keycloakRealm);

scalar.WithApiReference(ratingApi, options =>
{
    options.AddDocument("v1", "Rating API");
});
#endregion


#region ServiceReviewApi
var reviewdb = mongo.AddDatabase(AppHostProjects.ReviewDb, "reviewdb");

var reviewApiBearerAudience = builder.AddParameter("review-api-bearer-audience", value: "restorate-review-api", publishValueAsDefault: true);
var reviewApi = builder.AddProject<RestoRate_ReviewService_Api>(AppHostProjects.ServiceReviewApi)
    .WithReference(keycloak)
    .WithReference(rabbitmq)
    .WithReference(reviewdb)
    .WaitFor(keycloak).WaitFor(reviewdb)
    .WithEnvironment("KeycloakSettings__Audience", reviewApiBearerAudience)
    .WithEnvironment("KeycloakSettings__Realm", keycloakRealm);

scalar.WithApiReference(reviewApi, options =>
{
    options.AddDocument("v1", "Review API");
});
#endregion


#region gateway
var gatewayClientSecret = builder.AddParameter("gateway-client-secret", secret: true);
var gatewayClientId = builder.AddParameter("gateway-client-id", value: "restorate-gateway", publishValueAsDefault: true);
var gatewayAudience = builder.AddParameter("gateway-bearer-audience", value: "restorate-gateway", publishValueAsDefault: true);

var gateway = builder.AddProject<RestoRate_Gateway>(AppHostProjects.Gateway)
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithReference(restaurantApi)
    .WithReference(moderationApi)
    .WithReference(ratingApi)
    .WithReference(reviewApi)
    .WithEnvironment("KeycloakSettings__Audience", gatewayAudience)
    .WithEnvironment("KeycloakSettings__Realm", keycloakRealm)
    .WithEnvironment("KeycloakSettings__ClientId", gatewayClientId)
    .WithEnvironment("KeycloakSettings__ClientSecret", gatewayClientSecret);
#endregion


#region blazordashboard
var blazordashboardClientSecret = builder.AddParameter("blazordashboard-client-secret", secret: true);
var blazordashboardClientId = builder.AddParameter("blazordashboard-client-id", value: "restorate-dashboard", publishValueAsDefault: true);

builder.AddProject<RestoRate_BlazorDashboard>(AppHostProjects.BlazorDashboard)
    .WithReference(gateway)
    .WithReference(keycloak)
    .WaitFor(keycloak)
    .WithEnvironment("KeycloakSettings__ClientId", blazordashboardClientId)
    .WithEnvironment("KeycloakSettings__ClientSecret", blazordashboardClientSecret)
    .WithEnvironment("KeycloakSettings__Realm", keycloakRealm)
    .WithExternalHttpEndpoints();
#endregion

await builder.Build().RunAsync();
