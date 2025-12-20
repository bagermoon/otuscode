using RestoRate.BuildingBlocks.Data;
using RestoRate.BuildingBlocks.Data.Migrations;
using RestoRate.Migrations.RestaurantSeeders;
using RestoRate.RestaurantService.Infrastructure.Data;
using RestoRate.ServiceDefaults;
using RestoRate.SharedKernel.Diagnostics;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();
builder.Services
    .AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(ActivitySources.DbMigrations));

#region restaurant migrations
builder.AddPostgresDbContext<RestaurantDbContext>(AppHostProjects.RestaurantDb);
builder.Services.AddMigration<RestaurantDbContext, TagSeeder>();
builder.Services.AddMigration<RestaurantDbContext, RestaurantSeeder>();
#endregion


var host = builder.Build();
host.Run();
