using MassTransit;

using RestoRate.Abstractions.Identity;

namespace RestoRate.BuildingBlocks.Messaging.Identity;

public class HeaderUserContext : IUserContext
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = default!;
    public string FullName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public IReadOnlyCollection<string> Roles { get; init; } = [];
    public bool IsAuthenticated { get; init; }

    private HeaderUserContext()
    { }

    public static bool TryGetUserContext(ConsumeContext context, out HeaderUserContext userContext)
    {
        userContext = default!;
        if (IntegrationHeaderReader.TryGetUserId(context, out var userId))
        {
            userContext = new HeaderUserContext
            {
                UserId = userId,
                FullName = IntegrationHeaderReader.GetUserFullName(context) ?? string.Empty,
                Name = IntegrationHeaderReader.GetUserName(context) ?? string.Empty,
                Email = IntegrationHeaderReader.GetUserEmail(context) ?? string.Empty,
                Roles = IntegrationHeaderReader.GetUserRoles(context),
                IsAuthenticated = IntegrationHeaderReader.IsAuthenticated(context)
            };
            return true;
        }

        return false;
    }
}
