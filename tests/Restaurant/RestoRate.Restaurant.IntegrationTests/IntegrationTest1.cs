using Microsoft.Extensions.Logging;

using RestoRate.ServiceDefaults;

namespace RestoRate.Restaurant.IntegrationTests.Tests;

[Collection("AspireAppHost collection")]
public class IntegrationTest1(
    AspireAppHost host,
    ITestContextAccessor testContextAccessor
)
{
    [Fact]
    public async Task GetWeatherforecastErrortatusCode()
    {
        // Act
        var httpClient = host.CreateHttpClient(AppHostProjects.ServiceRestaurantApi, "https");
        var response = await httpClient.GetAsync("/weatherforecast", testContextAccessor.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
