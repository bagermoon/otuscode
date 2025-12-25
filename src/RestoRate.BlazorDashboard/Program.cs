using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Components.Server;

using MudBlazor.Services;

using RestoRate.Auth.Authentication;
using RestoRate.Auth.Identity;
using RestoRate.BlazorDashboard.Components;
using RestoRate.BlazorDashboard.Services;
using RestoRate.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// for blazor debugging
builder.Services.Configure<CircuitOptions>(opts => opts.DetailedErrors = true);

builder.Services.AddMudServices();

builder.AddCookieOidcAuthentication(AppHostProjects.Keycloak);
builder.Services.AddAuthorizationBuilder();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAntiforgery();
builder.Services.AddHttpContextAccessor();
builder.AddItentityServices();

builder.Services.AddTransient<TokenHandler>();

builder.Services.AddHttpClient(AppHostProjects.Gateway,
    client => client.BaseAddress = new Uri($"https+http://{AppHostProjects.Gateway}/api/"))
    .AddHttpMessageHandler<TokenHandler>();

builder.Services.AddScoped<RestaurantWebService>();

var app = builder.Build();

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found");

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapDefaultEndpoints();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGroup("/authentication")
    .MapCookieOidcAuthEndpoints();

app.Run();
