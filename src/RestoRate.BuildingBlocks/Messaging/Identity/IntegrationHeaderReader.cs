using System;

using MassTransit;

using RestoRate.Abstractions.Messaging;

namespace RestoRate.BuildingBlocks.Messaging.Identity;

public static class IntegrationHeaderReader
{
    public static bool TryGetUserId(ConsumeContext ctx, out Guid userId)
    {
        var raw = ctx.Headers?.Get<string>(IntegrationHeaders.UserId);
        return Guid.TryParse(raw, out userId);
    }

    public static string? GetUserName(ConsumeContext ctx)
        => ctx.Headers?.Get<string>(IntegrationHeaders.UserName);

    public static string? GetUserFullName(ConsumeContext ctx)
        => ctx.Headers?.Get<string>(IntegrationHeaders.UserFullName);

    public static string? GetUserEmail(ConsumeContext ctx)
        => ctx.Headers?.Get<string>(IntegrationHeaders.UserEmail);

    public static string[] GetUserRoles(ConsumeContext ctx)
    {
        var raw = ctx.Headers?.Get<string>(IntegrationHeaders.UserRoles);
        return string.IsNullOrWhiteSpace(raw) ? Array.Empty<string>() : raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    public static bool IsAuthenticated(ConsumeContext ctx)
    {
        // Headers may store bool as string or native bool; handle both.
        var rawBool = ctx.Headers?.Get<bool>(IntegrationHeaders.IsAuthenticated);
        if (rawBool.HasValue) return rawBool.Value;

        var rawStr = ctx.Headers?.Get<string>(IntegrationHeaders.IsAuthenticated);
        return bool.TryParse(rawStr, out var parsed) && parsed;
    }
}
