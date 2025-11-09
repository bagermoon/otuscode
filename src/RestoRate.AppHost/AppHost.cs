using RestoRate.ServiceDefaults;

using Scalar.Aspire;
using Projects;
using Microsoft.Extensions.Hosting;

// https://fiodar.substack.com/p/a-guide-to-securing-net-aspire-apps
// https://www.youtube.com/watch?v=_aCuwWiKncY
var builder = DistributedApplication.CreateBuilder(args);

#region PostgreSql

var postgresPassword = builder.AddParameter("postgres-password", secret: true);
var postgres = builder.AddPostgres("postgres",
        password: postgresPassword,
        port: builder.Environment.IsDevelopment() ? 55432 : null
     )
    .WithPgWeb()
    .WithImageTag("17.6")
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent);

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

#region keycloak
var keycloakUsername = builder.AddParameter("keycloak-username", "admin");
var keycloakPassword = builder.AddParameter("keycloak-password", "admin", secret: true);
var keycloakRealm = builder.AddParameter("keycloak-realm", "restorate");

var keycloak = builder.AddKeycloak(AppHostProjects.Keycloak,
    port: 8080,
    adminUsername: keycloakUsername,
    adminPassword: keycloakPassword
)
.WithRealmImport("realm-restorate.json")
.WithDataVolume("restorate-keycloak")
.WithExternalHttpEndpoints()
.WithLifetime(ContainerLifetime.Persistent);

#endregion

#region RabbitMQ
var rabbitUsername = builder.AddParameter("rabbitmq-username", "admin", secret: true);
var rabbitPassword = builder.AddParameter("rabbitmq-password", "admin", secret: true);

var rabbitmq = builder.AddRabbitMQ("messaging", userName: rabbitUsername, password: rabbitPassword)
    .WithImageTag("4.1")
    .WithDataVolume(isReadOnly: false)
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);

#endregion

#region Scalar

var scalar = builder.AddScalarApiReference(opts =>
{
    opts
        .WithTheme(ScalarTheme.Purple);
});

var consumer = builder.AddProject<RestoRate_MessageConsumer>("message-consumer")
    .WithReference(rabbitmq);

scalar.WithApiReference(consumer, options =>
{
    options.AddDocument("v1", "Consumer API");
});
#endregion

#region Migrations
var migrations = builder.AddProject<RestoRate_Migrations>("migrations")
    .WithReference(restaurantDb)
    .WaitFor(restaurantDb);
#endregion

#region ServiceRestaurantApi
var restaurantApiBearerAudience = builder.AddParameter("restaurant-api-bearer-audience", secret: false);
var restaurantApi = builder.AddProject<RestoRate_Restaurant_Api>(AppHostProjects.ServiceRestaurantApi)
    .WithReference(keycloak)
    .WithReference(restaurantDb)
    .WaitFor(keycloak).WaitFor(migrations)
    .WithEnvironment("KeycloakSettings__Audience", restaurantApiBearerAudience)
    .WithEnvironment("KeycloakSettings__Realm", keycloakRealm);
#endregion

#region ServiceModerationApi
var moderationApiBearerAudience = builder.AddParameter("moderation-api-bearer-audience", secret: false);
var moderationApi = builder.AddProject<RestoRate_Moderation_Api>(AppHostProjects.ServiceModerationApi)
    .WithReference(keycloak)
    .WaitFor(keycloak).WaitFor(migrations)
    .WithEnvironment("KeycloakSettings__Audience", moderationApiBearerAudience)
    .WithEnvironment("KeycloakSettings__Realm", keycloakRealm);
#endregion

#region ServiceRatingApi
var ratingApiBearerAudience = builder.AddParameter("rating-api-bearer-audience", secret: false);
var ratingApi = builder.AddProject<RestoRate_Rating_Api>(AppHostProjects.ServiceRatingApi)
    .WithReference(keycloak)
    .WaitFor(keycloak).WaitFor(migrations)
    .WithEnvironment("KeycloakSettings__Audience", ratingApiBearerAudience)
    .WithEnvironment("KeycloakSettings__Realm", keycloakRealm);
#endregion

#region ServiceReviewApi
var reviewApiBearerAudience = builder.AddParameter("review-api-bearer-audience", secret: false);
var reviewApi = builder.AddProject<RestoRate_Review_Api>(AppHostProjects.ServiceReviewApi)
    .WithReference(keycloak)
    .WaitFor(keycloak).WaitFor(migrations)
    .WithEnvironment("KeycloakSettings__Audience", reviewApiBearerAudience)
    .WithEnvironment("KeycloakSettings__Realm", keycloakRealm);
#endregion

#region gateway
var gatewayClientSecret = builder.AddParameter("gateway-client-secret", secret: true);
var gatewayClientId = builder.AddParameter("gateway-client-id");
var gatewayAudience = builder.AddParameter("gateway-bearer-audience", secret: false);

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
var blazordashboardClientId = builder.AddParameter("blazordashboard-client-id");

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
