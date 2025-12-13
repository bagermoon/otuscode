namespace RestoRate.Abstractions.Messaging;

/// <summary>
/// Standard header names used for propagating user identity and auth metadata with integration messages.
/// </summary>
public static class IntegrationHeaders
{
    public const string UserId = "x-user-id";
    public const string UserName = "x-user-name";
    public const string UserFullName = "x-user-full-name";
    public const string UserEmail = "x-user-email";
    public const string UserRoles = "x-user-roles";
    public const string IsAuthenticated = "x-authenticated";
}
