namespace RestoRate.Testing.Common.Auth;

public static class TestUsers
{
    private static readonly Dictionary<TestUser, TestUserInfo> _users = new()
    {
        [TestUser.Anonymous] = new("anonymous", [], "anonymous@example.com"),
        [TestUser.User] = new("user", ["user"], "user@example.com"),
        [TestUser.Admin] = new("admin", ["admin"], "admin@example.com")
    };

    public static TestUserInfo Get(TestUser type) => _users[type];
    public static IEnumerable<TestUser> All => _users.Keys;
    public record TestUserInfo(string Name, string[] Roles, string Email, string? Password = null);
}
