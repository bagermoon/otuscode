namespace RestoRate.Testing.Common.Auth;

public static class TestUsers
{
    private static readonly Dictionary<TestUser, TestUserInfo> _users = new()
    {
        [TestUser.Anonymous] = new(Guid.Empty, "anonymous", [], "anonymous@example.com"),
        [TestUser.User] = new(Guid.NewGuid(), "user", ["user"], "user@example.com", "user"),
        [TestUser.Admin] = new(Guid.NewGuid(), "admin", ["admin"], "admin@example.com", "admin")
    };

    public static TestUserInfo Get(TestUser type) => _users[type];
    public static IEnumerable<TestUser> All => _users.Keys;
    public record TestUserInfo(Guid Id, string Name, string[] Roles, string Email, string? Password = null);
}
