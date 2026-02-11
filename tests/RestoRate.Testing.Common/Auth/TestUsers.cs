namespace RestoRate.Testing.Common.Auth;

public static class TestUsers
{
    private static readonly Dictionary<TestUser, TestUserInfo> _users = new()
    {
        [TestUser.Anonymous] = new(
            Name: "anonymous",
            Roles: [],
            Email: "anonymous@example.com",
            UserId: Guid.Empty),
        [TestUser.User] = new(
            Name: "user",
            Roles: ["user"],
            Email: "user@example.com",
            UserId: new Guid("11111111-1111-1111-1111-111111111111"),
            Password: "user"),
        [TestUser.Admin] = new(
            Name: "admin",
            Roles: ["admin"],
            Email: "admin@example.com",
            UserId: new Guid("22222222-2222-2222-2222-222222222222"),
            Password: "admin")
    };

    public static TestUserInfo Get(TestUser type) => _users[type];
    public static IEnumerable<TestUser> All => _users.Keys;
    public record TestUserInfo(string Name, string[] Roles, string Email, Guid UserId, string? Password = null);
}
