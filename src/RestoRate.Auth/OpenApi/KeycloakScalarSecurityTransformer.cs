using Ardalis.GuardClauses;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.ServiceDiscovery;
using Microsoft.OpenApi.Models;

using RestoRate.Auth.Authentication;
using RestoRate.ServiceDefaults;

namespace RestoRate.Auth.OpenApi;

public class KeycloakScalarSecurityTransformer(
    IAuthenticationSchemeProvider authenticationSchemeProvider,
    IConfiguration configuration,
    ServiceEndpointResolver endpointResolver
)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        if (!authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            return;
        }

        document.Components ??= new OpenApiComponents();

        var securitySchemeBearer = "Bearer";

        document.Components.SecuritySchemes.Add(
            securitySchemeBearer,
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                In = ParameterLocation.Header
            }
        );

        var settings = new KeycloakSettingsOptions();
        configuration
            .GetSection(KeycloakSettingsOptions.SectionName)
            .Bind(settings);

        var keycloakEndpoint = await GetKeycloakHostUri(cancellationToken);
        var authorizationUrl = new Uri(keycloakEndpoint, $"/realms/{settings.Realm}/protocol/openid-connect/auth");
        var tokenUrl = new Uri($"{keycloakEndpoint.Scheme}://{AppHostProjects.Keycloak.ToLower()}/realms/{settings.Realm}/protocol/openid-connect/token");

        var securitySchemeOAuth2 = "OAuth2";
        document.Components.SecuritySchemes.Add(
            securitySchemeOAuth2,
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = authorizationUrl,
                        TokenUrl = tokenUrl,
                        Scopes = new Dictionary<string, string>()
                    },
                    ClientCredentials = new OpenApiOAuthFlow
                    {
                        Scopes = new Dictionary<string, string>(),
                        TokenUrl = tokenUrl,
                    }
                }
            }
        );
        // Add "Bearer" scheme as a requirement for the API as a whole
        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = securitySchemeBearer,
                        Type = ReferenceType.SecurityScheme
                    }
                }] = []
        });
        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            [
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = securitySchemeOAuth2,
                        Type = ReferenceType.SecurityScheme
                    }
                }] = []
        });
    }

    private async Task<Uri> GetKeycloakHostUri(CancellationToken cancellationToken)
    {
        var keycloakEndpoints = await endpointResolver.GetEndpointsAsync(
            $"https+http://{AppHostProjects.Keycloak}", cancellationToken: cancellationToken);

        var serviceEndpoint = keycloakEndpoints.Endpoints.Count > 0 ? keycloakEndpoints.Endpoints[0] : null;
        var serviceUri = serviceEndpoint?.EndPoint?.ToString();
        var isValidUri = !string.IsNullOrWhiteSpace(serviceUri) && serviceUri.Contains("://", StringComparison.Ordinal);

        return isValidUri
            ? new Uri(serviceUri!)
            : new Uri($"http://localhost:8080"); // When generate OpenAPI docs locally
    }
}
