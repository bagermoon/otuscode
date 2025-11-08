using RestoRate.BuildingBlocks.Data;
using RestoRate.BuildingBlocks.Data.Migrations;
using RestoRate.BuildingBlocks.Diagnostics;
using RestoRate.Common;
using RestoRate.Restaurant.Infrastructure.Data;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(ActivitySources.DbMigrations));

#region restaurant migrations
builder.AddPostgresDbContext<RestaurantDbContext>(AppHostProjects.RestaurantDb);
builder.Services.AddMigration<RestaurantDbContext>();
#endregion


var host = builder.Build();
host.Run();
