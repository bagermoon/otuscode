using System.IdentityModel.Tokens.Jwt;

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

builder.Services.AddMudServices();

builder.AddCookieOidcAuthentication(AppHostProjects.Keycloak);
builder.Services.AddAuthorizationBuilder();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAntiforgery();
builder.Services.AddHttpContextAccessor();
builder.AddItentityServices();

builder.Services.AddScoped<TokenHandler>();

builder.Services.AddHttpClient("RestaurantApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["RestaurantApiBaseUrl"]!);
})
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
