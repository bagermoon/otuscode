namespace RestoRate.Testing.Common.Auth;

public static class TestUsers
{
    private static readonly Dictionary<TestUser, TestUserInfo> _users = new()
    {
        [TestUser.Anonymous] = new("Anonymous", []),
        [TestUser.User] = new("TestUser", ["user"], "user"),
        [TestUser.Admin] = new("AdminUser", ["admin"], "admin")
    };

    public static TestUserInfo Get(TestUser type) => _users[type];
    public static IEnumerable<TestUser> All => _users.Keys;
    public record TestUserInfo(string Name, string[] Roles, string? Password = null);
}
