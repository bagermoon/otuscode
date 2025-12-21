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
// NOTE: Seeder order is important here:
// - TagSeeder must run before RestaurantSeeder because restaurants depend on tags being present.
// If you change this list (e.g., insert/remove/reorder seeders), keep this ordering constraint.
builder.Services.AddMigration<RestaurantDbContext, TagSeeder>();
builder.Services.AddMigration<RestaurantDbContext, RestaurantSeeder>();
#endregion


var host = builder.Build();
host.Run();
