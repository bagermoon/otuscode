namespace RestoRate.Abstractions.Messaging;

/// <summary>
/// Standard header names used for propagating user identity and auth metadata with integration messages.
/// </summary>
public static class IntegrationHeaders
{
    public const string UserContext = "x-user-context";
}
