using System.IdentityModel.Tokens.Jwt;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Caching.Memory;

namespace RestoRate.Gateway.Middleware.TokenExchange;

internal sealed class CachedTokenManager(IMemoryCache cache)
{
    private readonly IMemoryCache _cache = cache;
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    public static string BuildCacheKey(string scope, string incomingToken)
    {
        // Derive a compact token identifier: prefer iss|aud|jti, fall back to iss|aud|sub|iat, else hash the raw token.
        string tokenIdentifier;
        try
        {
            var handler = new JwtSecurityTokenHandler();
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
            tokenIdentifier = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(incomingToken)));
        }

        var bytes = Encoding.UTF8.GetBytes($"{scope}|{tokenIdentifier}");
        return "token-exchange:" + Convert.ToHexString(SHA256.HashData(bytes));
    }

    public bool TryGetToken(string cacheKey, out string? accessToken)
    {
        if (_cache.TryGetValue<string>(cacheKey, out accessToken) && !string.IsNullOrWhiteSpace(accessToken))
        {
            return true;
        }

        accessToken = null;
        return false;
    }

    public void SetToken(string cacheKey, string accessToken, int? expiresIn)
    {
        var expiresInSeconds = expiresIn.GetValueOrDefault(60);

        var cacheFor = TimeSpan.FromSeconds(Math.Max(5, expiresInSeconds - 30));
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = cacheFor
        }.RegisterPostEvictionCallback((key, _, _, _) =>
        {
            _keys.TryRemove((string)key!, out _);
        });
        _cache.Set(cacheKey, accessToken, options);
        _keys.TryAdd(cacheKey, 0);
    }

    public void ClearAll()
    {
        var keys = _keys.Keys.ToArray();
        foreach (var key in keys)
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }
    }
}
