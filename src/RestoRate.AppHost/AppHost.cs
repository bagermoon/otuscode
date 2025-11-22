using Aspire.Hosting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Projects;
using RestoRate.ServiceDefaults;
using Scalar.Aspire;

// https://fiodar.substack.com/p/a-guide-to-securing-net-aspire-apps
// https://www.youtube.com/watch?v=_aCuwWiKncY
var builder = DistributedApplication.CreateBuilder(args);

var isProduction = builder.Environment.IsProduction();
var useVolumes = builder.Configuration.GetValue("AppHostConfiguration:UseVolumes", true);
var useDedicatedPorts = builder.Configuration.GetValue("AppHostConfiguration:UseDedicatedPorts", true)
    && !isProduction;

var usePgWeb = builder.Configuration.GetValue("AppHostConfiguration:UsePgWeb", true)
    && !isProduction;

var usePostgresLifetimePersistent = builder.Configuration.GetValue("AppHostConfiguration:UsePostgresLifetimePersistent", true);
var usedPostgresImageTag = builder.Configuration.GetValue("AppHostConfiguration:UsePostgresImageTag", "17.6");

var useMongoExpress = builder.Configuration.GetValue("AppHostConfiguration:UseMongoExpress", true);
var usedMongoImageTag = builder.Configuration.GetValue("AppHostConfiguration:UseMongoImageTag", "8.2");
var useMongoLifetimePersistent = builder.Configuration.GetValue("AppHostConfiguration:UseMongoLifetimePersistent", true);

var useRedisImageTag = builder.Configuration.GetValue("AppHostConfiguration:UseRedisImageTag", "8.2");
var useRedisInsight = builder.Configuration.GetValue("AppHostConfiguration:UseRedisInsight", true);
var useRedisLifetimePersistent = builder.Configuration.GetValue("AppHostConfiguration:UseRedisLifetimePersistent", true);

var useKeycloakLocalPort = builder.Configuration.GetValue<int?>("AppHostConfiguration:UseKeycloakLocalPort", 8080);
var useKeycloakImageTag = builder.Configuration.GetValue("AppHostConfiguration:UseKeycloakImageTag", "26.3");
var useKeycloakLifetimePersistent = builder.Configuration.GetValue("AppHostConfiguration:UseKeycloakLifetimePersistent", true);

var useRabbitImageTag = builder.Configuration.GetValue("AppHostConfiguration:UseRabbitImageTag", "4.1");
var useRabbitLifetimePersistent = builder.Configuration.GetValue("AppHostConfiguration:UseRabbitLifetimePersistent", true);
var useRabbitManagementPlugin = builder.Configuration.GetValue("AppHostConfiguration:UseRabbitManagementPlugin", true);

var parameters = builder.Configuration.GetRequiredSection("Parameters");

#region PostgreSql

var postgres = builder.AddPostgres("postgres",
        password: builder.AddParameter("postgres-password", secret: true),
        port: useDedicatedPorts ? 55432 : null
     )
    .WithImageTag(usedPostgresImageTag)
    .WithLifetime(usePostgresLifetimePersistent ? ContainerLifetime.Persistent : ContainerLifetime.Session);

if (useVolumes)
{
    postgres.WithDataVolume();
}

if (usePgWeb)
{
    postgres.WithPgWeb();
}

var restaurantDb = postgres.AddDatabase(AppHostProjects.RestaurantDb, "Restaurants");


/**

int portPostgreSql = 46819;
Regex re = new Regex(@"^(.*[/\\]otuscode)[/\\]{0,1}.*$");
var r = re.Matches(Environment.CurrentDirectory);
string root_directory = r.Count > 0 ? r[0].Groups[1].Value + "/src/DataBase" : null;
string data_directory = root_directory + "/restaurants";

var initDirs = builder.AddContainer("create-structure", "alpine:latest")
.WithArgs("sh", "-c", $"""
                       mkdir -p /var/lib/postgresql/restaurants_tbs
                       chown -R 999:1000 /var/lib/postgresql/restaurants_tbs
                       """)
.WithBindMount(data_directory, "/var/lib/postgresql");

var postgres = builder.AddPostgres("postgres", port: portPostgreSql)
.WithDataBindMount(data_directory + "/data")
.WithBindMount(data_directory + "/restaurants_tbs","/var/lib/postgresql/restaurants_tbs")
.WithInitFiles(root_directory + "/init-scripts");

var RestaurantsDbConnectionString = new NpgsqlConnectionStringBuilder()
{
    Host = "localhost",
    Port = portPostgreSql,
    Database = "Restaurants",
    Username = "master",
    Password = "master"
}.ToString();

var RestaurantsDb = builder.AddConnectionString("RestaurantsDb", RestaurantsDbConnectionString);
*/

#endregion

# region mongodb

var mongo = builder.AddMongoDB(
        name: "mongo",
        userName: builder.AddParameter("mongo-username", value: "admin", publishValueAsDefault: true),
        password: builder.AddParameter("mongo-password", secret: true)
    )
    .WithImageTag(usedMongoImageTag)
    .WithLifetime(useMongoLifetimePersistent ? ContainerLifetime.Persistent : ContainerLifetime.Session);

if (useMongoExpress)
{
    mongo.WithMongoExpress(res =>
    {
        if (useDedicatedPorts)
        {
            res.WithHostPort(27017);
        }
    });
}
if (useVolumes)
{
    mongo.WithDataVolume();
}
#endregion

# region redis
var redisCache = builder.AddRedis(AppHostProjects.RedisCache,
    port: useDedicatedPorts ? 6380 : null)
    .WithImageTag(useRedisImageTag)
    .WithLifetime(useRedisLifetimePersistent ? ContainerLifetime.Persistent : ContainerLifetime.Session);

if (useVolumes)
{
    redisCache.WithDataVolume();
}

if (useRedisInsight)
{
    redisCache.WithRedisInsight(res =>
    {
        if (useDedicatedPorts)
        {
            res.WithHostPort(8001);
        }
    });
}
#endregion

#region keycloak
var keycloakRealm = builder.AddParameter("keycloak-realm", value: "restorate", publishValueAsDefault: true);
var keycloakHostname = builder.AddParameter("keycloak-hostname");

var keycloak = builder.AddKeycloak(AppHostProjects.Keycloak,
    port: useKeycloakLocalPort ?? default,
    adminUsername: builder.AddParameter("keycloak-username", value: "admin", publishValueAsDefault: true),
    adminPassword: builder.AddParameter("keycloak-password", secret: true)
)
.WithImageTag(useKeycloakImageTag)
.WithRealmImport("restorate-realm.json")
.WithEnvironment("KC_HOSTNAME", keycloakHostname)
.WithExternalHttpEndpoints()
.WithLifetime(useKeycloakLifetimePersistent ? ContainerLifetime.Persistent : ContainerLifetime.Session);

if (useVolumes)
{
    keycloak.WithDataVolume("restorate-keycloak");
}

#endregion

#region RabbitMQ
var rabbitmq = builder.AddRabbitMQ(
        AppHostProjects.RabbitMQ,
        userName: builder.AddParameter("rabbitmq-username", value: "guest", publishValueAsDefault: true),
        password: builder.AddParameter("rabbitmq-password", secret: true)
    )
    .WithImageTag(useRabbitImageTag)
    .WithLifetime(useRabbitLifetimePersistent ? ContainerLifetime.Persistent : ContainerLifetime.Session);

if (useVolumes)
{
    rabbitmq.WithDataVolume();
}

if (useRabbitManagementPlugin)
{
    rabbitmq.WithManagementPlugin();
}

#endregion

#region Scalar

var apiClientId = builder.AddParameter("api-client-id", value: "restorate-api", publishValueAsDefault: true);
var apiClientSecret = builder.AddParameter("api-client-secret", secret: true);

var scalar = builder.AddScalarApiReference(opts =>
{
    opts
        .WithTheme(ScalarTheme.Purple);

    opts
        .AddPreferredSecuritySchemes("OAuth2")
        .AddAuthorizationCodeFlow("OAuth2", flow =>
        {
            var idValue = apiClientId.Resource
                .GetValueAsync(default).AsTask().GetAwaiter().GetResult()!;
            var secretValue = apiClientSecret.Resource
                .GetValueAsync(default).AsTask().GetAwaiter().GetResult()!;
            flow
                .WithClientId(idValue)
                .WithClientSecret(secretValue);
        })
        .AddClientCredentialsFlow("OAuth2", flow =>
        {
            var idValue = apiClientId.Resource
                .GetValueAsync(default).AsTask().GetAwaiter().GetResult()!;
            var secretValue = apiClientSecret.Resource
                .GetValueAsync(default).AsTask().GetAwaiter().GetResult()!;
            flow
                .WithClientId(idValue)
                .WithClientSecret(secretValue);
        });
})
.WithReference(keycloak);
scalar.WithExplicitStart();

#endregion

#region Migrations
var migrations = builder.AddProject<RestoRate_Migrations>("migrations")
    .WithReference(restaurantDb)
    .WaitFor(restaurantDb);
#endregion

#region ServiceRestaurantApi
var restaurantApiBearerAudience = builder.AddParameter("restaurant-api-bearer-audience", value: "restorate-restaurant-api", publishValueAsDefault: true);
var restaurantApi = builder.AddProject<RestoRate_Restaurant_Api>(AppHostProjects.ServiceRestaurantApi)
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
var moderationApi = builder.AddProject<RestoRate_Moderation_Api>(AppHostProjects.ServiceModerationApi)
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
var ratingApi = builder.AddProject<RestoRate_Rating_Api>(AppHostProjects.ServiceRatingApi)
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
var reviewApi = builder.AddProject<RestoRate_Review_Api>(AppHostProjects.ServiceReviewApi)
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
