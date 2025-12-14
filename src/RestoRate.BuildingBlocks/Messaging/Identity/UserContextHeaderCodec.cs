using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using MassTransit;

using RestoRate.Abstractions.Identity;
using RestoRate.Abstractions.Messaging;

namespace RestoRate.BuildingBlocks.Messaging.Identity;

public static class UserContextHeaderCodec
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions DeserializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static bool TryWrite(SendHeaders headers, IUserContext userContext, bool overwriteExisting = false)
    {
        if (headers is null) return false;
        if (userContext is null) return false;
        if (!userContext.IsAuthenticated) return false;
        if (userContext.UserId == Guid.Empty) return false;

        if (!overwriteExisting && headers.TryGetHeader(IntegrationHeaders.UserContext, out _))
            return false;

        var dto = new UserContextHeaderDto(
            userContext.UserId,
            userContext.Name,
            userContext.FullName,
            userContext.Email,
            userContext.Roles?.ToArray(),
            userContext.IsAuthenticated
        );

        var json = JsonSerializer.Serialize(dto, SerializerOptions);
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        headers.Set(IntegrationHeaders.UserContext, encoded);
        return true;
    }

    public static bool TryRead(Headers headers, out HeaderUserContext userContext)
    {
        userContext = default!;
        if (headers is null) return false;

        if (!headers.TryGetHeader(IntegrationHeaders.UserContext, out var raw) || raw is null)
            return false;

        if (!TryExtractString(raw, out var encoded) || string.IsNullOrWhiteSpace(encoded))
            return false;

        if (!TryDecode(encoded, out var dto) || dto is null)
            return false;

        if (!dto.IsAuthenticated || dto.UserId == Guid.Empty)
            return false;

        userContext = new HeaderUserContext
        {
            UserId = dto.UserId,
            Name = dto.Name ?? string.Empty,
            FullName = dto.FullName ?? string.Empty,
            Email = dto.Email ?? string.Empty,
            Roles = dto.Roles ?? Array.Empty<string>(),
            IsAuthenticated = dto.IsAuthenticated
        };

        return true;
    }

    private static bool TryDecode(string encoded, out UserContextHeaderDto? dto)
    {
        dto = null;
        try
        {
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            dto = JsonSerializer.Deserialize<UserContextHeaderDto>(json, DeserializerOptions);
            return dto is not null;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryExtractString(object raw, out string? value)
    {
        value = raw as string;
        if (value is not null) return true;

        if (raw is byte[] bytes)
        {
            value = Encoding.UTF8.GetString(bytes);
            return true;
        }

        if (raw is ReadOnlyMemory<byte> rom)
        {
            value = Encoding.UTF8.GetString(rom.Span);
            return true;
        }

        return false;
    }
    private record UserContextHeaderDto(
        Guid UserId,
        string? Name,
        string? FullName,
        string? Email,
        string[]? Roles,
        bool IsAuthenticated
    );
}
