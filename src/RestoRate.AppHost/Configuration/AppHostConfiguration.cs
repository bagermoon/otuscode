namespace RestoRate.AppHost.Configuration;

public class AppHostConfiguration
{
    public bool UseVolumes { get; set; }
    public bool UseDedicatedPorts { get; set; }
    public PostgresConfig Postgres { get; set; } = new();
    public MongoConfig Mongo { get; set; } = new();
    public RedisConfig Redis { get; set; } = new();
    public KeycloakConfig Keycloak { get; set; } = new();
    public RabbitConfig Rabbit { get; set; } = new();
    public ScalarConfig Scalar { get; set; } = new();
}

public class PostgresConfig
{
    public bool LifetimePersistent { get; set; }
    public string ImageTag { get; set; } = "";
    public bool UsePgWeb { get; set; }
}

public class MongoConfig
{
    public bool UseExpress { get; set; }
    public string ImageTag { get; set; } = "";
    public bool LifetimePersistent { get; set; }
}

public class RedisConfig
{
    public string ImageTag { get; set; } = "";
    public bool UseInsight { get; set; }
    public bool LifetimePersistent { get; set; }
}

public class KeycloakConfig
{
    public int? LocalPort { get; set; }
    public string ImageTag { get; set; } = "";
    public bool LifetimePersistent { get; set; }
}

public class RabbitConfig
{
    public string ImageTag { get; set; } = "";
    public bool LifetimePersistent { get; set; }
    public bool UseManagementPlugin { get; set; }
}

public class ScalarConfig
{
    public bool ExplicitStart { get; set; }
}