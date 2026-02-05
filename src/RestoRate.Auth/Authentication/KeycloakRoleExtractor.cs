using System.Security.Claims;
using System.Text.Json;

namespace RestoRate.Auth.Authentication;

internal static class KeycloakRoleExtractor
{
    public static void AddRolesFromAccessToken(ClaimsIdentity identity, string? jwtAccessToken, string? clientId)
    {
        var roles = ExtractRoles(jwtAccessToken, clientId);

        foreach (var role in roles)
        {
            if (!identity.HasClaim(identity.RoleClaimType, role))
                identity.AddClaim(new Claim(identity.RoleClaimType, role));
        }
    }
    public static IReadOnlyCollection<string> ExtractRoles(string? jwtAccessToken, string? clientId)
    {
        if (string.IsNullOrWhiteSpace(jwtAccessToken)) return [];

        var parts = jwtAccessToken.Split('.');
        if (parts.Length < 2) return [];

        try
        {
            var payloadBytes = Base64UrlDecode(parts[1]);
            using var doc = JsonDocument.Parse(payloadBytes);

            var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1) If gateway/token-exchange already flattens roles: "roles": ["admin", ...]
            AddStringArray(doc.RootElement, roles, "roles");

            // 2) Keycloak default structures
            if (doc.RootElement.TryGetProperty("realm_access", out var realmAccess)
                && realmAccess.ValueKind == JsonValueKind.Object)
            {
                AddStringArray(realmAccess, roles, "roles");
            }

            if (!string.IsNullOrWhiteSpace(clientId)
                && doc.RootElement.TryGetProperty("resource_access", out var resourceAccess)
                && resourceAccess.ValueKind == JsonValueKind.Object
                && resourceAccess.TryGetProperty(clientId, out var clientAccess)
                && clientAccess.ValueKind == JsonValueKind.Object)
            {
                AddStringArray(clientAccess, roles, "roles");
            }

            roles.RemoveWhere(string.IsNullOrWhiteSpace);
            return roles.Count == 0 ? [] : roles.ToArray();
        }
        catch
        {
            return [];
        }
    }

    private static void AddStringArray(JsonElement element, HashSet<string> target, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var arr) || arr.ValueKind != JsonValueKind.Array)
            return;

        foreach (var r in arr.EnumerateArray())
        {
            if (r.ValueKind == JsonValueKind.String)
                target.Add(r.GetString()!);
        }
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }

        return Convert.FromBase64String(s);
    }
}
