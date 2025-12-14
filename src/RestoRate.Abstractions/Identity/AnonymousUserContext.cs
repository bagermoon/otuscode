namespace RestoRate.Abstractions.Identity;

public sealed class AnonymousUserContext : IUserContext
{
    public static readonly AnonymousUserContext Instance = new();

    private AnonymousUserContext() { }

    public Guid UserId => Guid.Empty;
    public string Name => string.Empty;
    public string FullName => string.Empty;
    public bool IsAuthenticated => false;
    public string Email => string.Empty;
    public IReadOnlyCollection<string> Roles => [];
}
