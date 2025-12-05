using System.IdentityModel.Tokens.Jwt;

using RestoRate.Auth.Authentication;
using RestoRate.Auth.Identity;
using RestoRate.BlazorDashboard.Components;
using RestoRate.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.AddCookieOidcAuthentication(AppHostProjects.Keycloak);
builder.Services.AddAuthorizationBuilder();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAntiforgery();
builder.Services.AddHttpContextAccessor();
builder.AddItentityServices();

builder.Services.AddScoped<TokenHandler>();

builder.Services.AddHttpClient(AppHostProjects.Gateway,
      client => client.BaseAddress = new Uri($"https+http://{AppHostProjects.Gateway}"))
      .AddHttpMessageHandler<TokenHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
