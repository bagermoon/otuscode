using RestoRate.ServiceDefaults;
using RestoRate.Auth.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetailsDefaults();
builder.Services.AddOpenApi(opts => opts.AddDocumentTransformer<KeycloakScalarSecurityTransformer>());
builder.AddModerationApi();

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
