using System;

namespace RestoRate.Abstractions.Identity;

public interface IUserContext
{
    string? UserId { get; }
    Guid? UserGuid { get; }
    bool IsAuthenticated { get; }
}
