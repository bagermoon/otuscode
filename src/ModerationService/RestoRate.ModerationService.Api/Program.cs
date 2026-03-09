using RestoRate.ServiceDefaults;
using RestoRate.Auth.Authentication;
using RestoRate.ServiceDefaults.EndpointFilters;
using RestoRate.ModerationService.Application;
using RestoRate.ModerationService.Infrastructure;
using RestoRate.ModerationService.Infrastructure.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetailsDefaults();

builder.Services.AddModerationApplication();
builder.AddModerationInfrastructure();

builder.AddJwtAuthentication(AppHostProjects.Keycloak);
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminGroup", policy =>
        policy.RequireRole("admin")); // Checks for a "roles" claim with value "admin"

builder.Services.Configure<ModerationSettingsOptions>(
            builder.Configuration.GetSection("ModerationSettings")
        );
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
    app.UseExceptionHandler();
}
else
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePages();
app.MapDefaultEndpoints();

app.Run();
