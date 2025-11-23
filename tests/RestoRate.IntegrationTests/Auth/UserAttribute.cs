namespace RestoRate.IntegrationTests.Auth;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class UserAttribute : Attribute
{
    public TestUser Name { get; }
    public UserAttribute(TestUser name)
    {
        Name = name;
    }
}
