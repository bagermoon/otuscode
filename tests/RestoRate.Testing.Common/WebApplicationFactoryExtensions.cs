using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using RestoRate.Testing.Common.Auth;

namespace RestoRate.Testing.Common;

public static class WebApplicationFactoryExtensions
{
    public static HttpClient CreateClientWithUser<TEntryPoint>(
        this WebApplicationFactory<TEntryPoint> factory,
        TestUser user
        ) where TEntryPoint : class
    {
        if (user == TestUser.Anonymous)
        {
            return factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });
        }

        var _factory = WithUser(factory, user);

        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
    }

    public static WebApplicationFactory<TEntryPoint> WithUser<TEntryPoint>(
        this WebApplicationFactory<TEntryPoint> factory,
        TestUser user
        ) where TEntryPoint : class
    {
        if (user == TestUser.Anonymous) return factory;

        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication("TestScheme")
                    .AddScheme<TestAuthOptions, TestAuthHandler>("TestScheme", o => o.User = user);
            });
        });
    }

    public static (WebApplicationFactory<TEntryPoint> Factory, HttpClient Client) CreateFactoryAndClientWithUser<TEntryPoint>(
        this WebApplicationFactory<TEntryPoint> factory,
        TestUser user
        ) where TEntryPoint : class
    {
        var f = factory.WithUser(user);
        return (f, f.CreateClient(new() { AllowAutoRedirect = false }));
    }
}
