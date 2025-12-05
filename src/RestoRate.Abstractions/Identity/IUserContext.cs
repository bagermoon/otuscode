using System;

namespace RestoRate.Abstractions.Identity;

public interface IUserContext
{
    Guid UserId { get; }
    string Name { get; }
    string FullName { get; }
    bool IsAuthenticated { get; }
    string Email { get; }
    IReadOnlyCollection<string> Roles { get; }
}
