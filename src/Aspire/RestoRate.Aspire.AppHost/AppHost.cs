using Aspire.Hosting;

using Projects;

using Scalar.Aspire;

// https://fiodar.substack.com/p/a-guide-to-securing-net-aspire-apps
// https://www.youtube.com/watch?v=_aCuwWiKncY
var builder = DistributedApplication.CreateBuilder(args);

var rabbitUsername = builder.AddParameter("rabbitmq-username", "admin", secret: true);
var rabbitPassword = builder.AddParameter("rabbitmq-password", "admin", secret: true);

var rabbitmq = builder.AddRabbitMQ("messaging", userName: rabbitUsername, password: rabbitPassword)
    .WithImageTag("4.1")
    .WithDataVolume(isReadOnly: false)
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);

rabbitUsername.WithParentRelationship(rabbitmq);
rabbitPassword.WithParentRelationship(rabbitmq);

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

var keycloakUsername = builder.AddParameter("keycloak-username", secret: true);
var keycloakPassword = builder.AddParameter("keycloak-password", secret: true);

var keycloak = builder.AddKeycloak("keycloak", 8080,
        adminUsername: keycloakUsername,
        adminPassword: keycloakPassword
    )
    .WithImageTag("26.3")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume();

keycloakUsername.WithParentRelationship(keycloak);
keycloakPassword.WithParentRelationship(keycloak);

await builder.Build().RunAsync();
