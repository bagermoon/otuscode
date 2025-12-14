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

    internal HeaderUserContext()
    { }
}
