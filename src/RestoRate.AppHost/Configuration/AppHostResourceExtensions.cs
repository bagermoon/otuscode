using Scalar.Aspire;
using RestoRate.AppHost.Configuration;

public static class AppHostResourceExtensions
{
    public static IResourceBuilder<PostgresServerResource> AddPostgresResource(
        this IDistributedApplicationBuilder builder, PostgresConfig config, bool useVolumes, bool useDedicatedPorts)
    {
        var postgres = builder.AddPostgres("postgres",
            password: builder.AddParameter("postgres-password", secret: true),
            port: useDedicatedPorts ? 55432 : null
        )
        .WithImageTag(config.ImageTag)
        .WithLifetime(config.LifetimePersistent ? ContainerLifetime.Persistent : ContainerLifetime.Session);

        if (useVolumes)
        {
            postgres.WithDataVolume();
        }

        if (config.UsePgWeb)
        {
            postgres.WithPgWeb();
        }

        return postgres;
    }

    public static IResourceBuilder<MongoDBServerResource> AddMongoResource(
        this IDistributedApplicationBuilder builder, MongoConfig config, bool useVolumes, bool useDedicatedPorts)
    {
        var mongo = builder.AddMongoDB(
            name: "mongo",
            userName: builder.AddParameter("mongo-username", value: "admin", publishValueAsDefault: true),
            password: builder.AddParameter("mongo-password", secret: true)
        )
        .WithImageTag(config.ImageTag)
        .WithLifetime(config.LifetimePersistent ? ContainerLifetime.Persistent : ContainerLifetime.Session);

        if (config.UseExpress)
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
        return mongo;
    }

    public static IResourceBuilder<RedisResource> AddRedisResource(
        this IDistributedApplicationBuilder builder,
        string name,
        RedisConfig config,
        bool useVolumes,
        bool useDedicatedPorts)
    {
        var redis = builder.AddRedis(name,
            port: useDedicatedPorts ? 6380 : null)
            .WithImageTag(config.ImageTag)
            .WithLifetime(config.LifetimePersistent ? ContainerLifetime.Persistent : ContainerLifetime.Session);
        if (useVolumes)
        {
            redis.WithDataVolume();
        }

        if (config.UseInsight)
        {
            redis.WithRedisInsight(res =>
            {
                if (useDedicatedPorts)
                {
                    res.WithHostPort(8001);
                }
            });
        }
        return redis;
    }

    public static IResourceBuilder<KeycloakResource> AddKeycloakResource(
        this IDistributedApplicationBuilder builder, KeycloakConfig config, bool useVolumes, bool useDedicatedPorts)
    {
        var keycloak = builder.AddKeycloak("keycloak",
            port: useDedicatedPorts ? (config.LocalPort ?? 8080) : default,
            adminUsername: builder.AddParameter("kc-username", value: "admin", publishValueAsDefault: true),
            adminPassword: builder.AddParameter("kc-password", secret: true)
        )
        .WithImageTag(config.ImageTag)
        .WithRealmImport("restorate-realm.json")

        .WithExternalHttpEndpoints()
        .WithLifetime(config.LifetimePersistent ? ContainerLifetime.Persistent : ContainerLifetime.Session);

        if (useVolumes)
        {
            keycloak.WithDataVolume("restorate-keycloak");
        }
        // Тема для dashboard (временное решение)
        keycloak.WithBindMount("keycloak-themes/restorate", "/opt/keycloak/themes/restorate", isReadOnly: false)
            .WithEnvironment("KC_SPI_THEME_STATIC_MAX_AGE", "-1")
            .WithEnvironment("KC_SPI_THEME_CACHE_THEMES", "false")
            .WithEnvironment("KC_SPI_THEME_CACHE_TEMPLATES", "false");

        if (config.UseKCHostname)
        {
            keycloak.WithEnvironment("KC_HOSTNAME", builder.AddParameter("kc-hostname"));
        }

        return keycloak;
    }

    public static IResourceBuilder<RabbitMQServerResource> AddRabbitResource(
        this IDistributedApplicationBuilder builder, RabbitConfig config, bool useVolumes)
    {
        var rabbitmq = builder.AddRabbitMQ("rabbitmq",
            userName: builder.AddParameter("rabbitmq-username", value: "guest", publishValueAsDefault: true),
            password: builder.AddParameter("rabbitmq-password", secret: true)
        )
        .WithImageTag(config.ImageTag)
        .WithLifetime(config.LifetimePersistent ? ContainerLifetime.Persistent : ContainerLifetime.Session);

        if (useVolumes)
        {
            rabbitmq.WithDataVolume();
        }

        if (config.UseManagementPlugin)
        {
            rabbitmq.WithManagementPlugin();
        }
        return rabbitmq;
    }

    public static IResourceBuilder<ScalarResource> AddScalarApiReferenceResource(
        this IDistributedApplicationBuilder builder, ScalarConfig config)
    {
        var apiClientId = builder.AddParameter("api-client-id", value: "restorate-api", publishValueAsDefault: true);
        var apiClientSecret = builder.AddParameter("api-client-secret", secret: true);

        var scalar = builder.AddScalarApiReference(opts =>
        {
            opts
                .WithTheme(ScalarTheme.Purple)
                .AddPreferredSecuritySchemes("OAuth2")
                .AddAuthorizationCodeFlow("OAuth2", flow =>
                {
                    var idValue = apiClientId.Resource.GetValueAsync(default).AsTask().GetAwaiter().GetResult()!;
                    var secretValue = apiClientSecret.Resource.GetValueAsync(default).AsTask().GetAwaiter().GetResult()!;
                    flow.WithClientId(idValue).WithClientSecret(secretValue);
                })
                .AddClientCredentialsFlow("OAuth2", flow =>
                {
                    var idValue = apiClientId.Resource.GetValueAsync(default).AsTask().GetAwaiter().GetResult()!;
                    var secretValue = apiClientSecret.Resource.GetValueAsync(default).AsTask().GetAwaiter().GetResult()!;
                    flow.WithClientId(idValue).WithClientSecret(secretValue);
                });
        });
        if (config.ExplicitStart)
        {
            scalar.WithExplicitStart();
        }
        return scalar;
    }
}
