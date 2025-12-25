using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Caching.Memory;

namespace RestoRate.Auth.Authentication.ClientCredentials;

internal sealed class CachedClientTokenManager(
    IMemoryCache cache
)
{
    private readonly IMemoryCache _cache = cache;

    private readonly ConcurrentDictionary<string, byte> _keys = new();

    public static string BuildCacheKey(string tokenEndpoint, string clientId)
    {
        // Hash to avoid very long keys and to normalize formatting.
        var keyHash = ComputeSha256Hex($"{tokenEndpoint}|{clientId}");
        return $"keycloak:client-credentials:{keyHash}";
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

    private static string ComputeSha256Hex(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);

        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }
}
