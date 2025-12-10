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

        return factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication("TestScheme")
                    .AddScheme<TestAuthOptions, TestAuthHandler>("TestScheme", options =>
                    {
                        options.User = user;
                    });
            });
        })
        .CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });
    }
}
