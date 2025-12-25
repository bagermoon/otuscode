using System.Security.Cryptography;
using System.Text;

namespace RestoRate.Auth.Authentication;

public static class TokenCacheKeyHelper
{
    private const string Prefix = "keycloak";
    public static string ClientCredentialsKey(string tokenEndpoint, string clientId) =>
        $"{Prefix}:client-credentials:{Sha256HexUtf8($"{tokenEndpoint}|{clientId}")}";


    public static string TokenExchangeKey(string scope, string incomingToken)
    {
        // Derive a compact token identifier: prefer iss|aud|jti, fall back to iss|aud|sub|iat, else hash the raw token.
        string tokenIdentifier;
        try
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(incomingToken);
            var iss = jwt.Issuer ?? string.Empty;
            var aud = string.Join(",", jwt.Audiences?.OrderBy(a => a) ?? Enumerable.Empty<string>());

            if (!string.IsNullOrEmpty(jwt.Id))
            {
                tokenIdentifier = $"{iss}|{aud}|{jwt.Id}";
            }
            else
            {
                var sub = jwt.Subject ?? string.Empty;
                var iat = jwt.Payload.TryGetValue("iat", out var iatVal) ? iatVal?.ToString() ?? string.Empty : string.Empty;
                tokenIdentifier = $"{iss}|{aud}|{sub}|{iat}";
            }
        }
        catch
        {
            // If parsing fails, hash the raw token to avoid using secrets in keys
            tokenIdentifier = Sha256HexUtf8(incomingToken);
        }

        return $"{Prefix}:token-exchange:{Sha256HexUtf8($"{scope}|{tokenIdentifier}")}";
    }

    private static string Sha256HexUtf8(string input) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
}
