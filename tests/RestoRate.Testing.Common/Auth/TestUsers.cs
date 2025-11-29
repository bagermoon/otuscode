namespace RestoRate.Testing.Common.Auth;

public static class TestUsers
{
    private static readonly Dictionary<TestUser, TestUserInfo> _users = new()
    {
        [TestUser.Anonymous] = new("anonymous", []),
        [TestUser.User] = new("user", ["user"], "user"),
        [TestUser.Admin] = new("admin", ["admin"], "admin")
    };

    public static TestUserInfo Get(TestUser type) => _users[type];
    public static IEnumerable<TestUser> All => _users.Keys;
    public record TestUserInfo(string Name, string[] Roles, string? Password = null);
}
