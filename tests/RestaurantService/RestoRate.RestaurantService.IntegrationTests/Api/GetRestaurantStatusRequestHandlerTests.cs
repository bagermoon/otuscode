using FluentAssertions;

using MassTransit;
using MassTransit.Testing;

using Microsoft.AspNetCore.Mvc.Testing;

using RestoRate.Contracts.Restaurant;
using RestoRate.Contracts.Restaurant.DTOs;
using RestoRate.Contracts.Restaurant.Requests;
using RestoRate.RestaurantService.IntegrationTests.Helpers;

namespace RestoRate.RestaurantService.IntegrationTests.Api;

public class GetRestaurantStatusRequestHandlerTests : IClassFixture<RestaurantWebApplicationFactory>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestContextAccessor _testContextAccessor;

    private CancellationToken CancellationToken => _testContextAccessor.Current.CancellationToken;

    public GetRestaurantStatusRequestHandlerTests(
        RestaurantWebApplicationFactory factory,
        ITestContextAccessor testContextAccessor)
    {
        (_factory, _client) = factory.CreateFactoryAndClientWithUser(TestUser.Admin);
        _testContextAccessor = testContextAccessor;
    }

    [Fact]
    public async Task GetRestaurantStatusRequest_Returns_ExistsTrue_And_DraftStatus_ForExistingRestaurant()
    {
        var harness = _factory.Services.GetRequiredService<ITestHarness>();
        await harness.Start();
        try
        {
            // Arrange: create a restaurant via HTTP API (persists to Postgres)
            var createRequest = RestaurantTestData.CreateValidRestaurantRequest("Для статуса");
            var createResponse = await _client.PostAsJsonAsync("/restaurants", createRequest, CancellationToken);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await createResponse.Content.ReadFromJsonAsync<RestaurantDto>(CancellationToken);
            created.Should().NotBeNull();

            // Act: ask RestaurantService over MassTransit request/response
            var response = await harness.Bus.Request<GetRestaurantStatusRequest, GetRestaurantStatusResponse>(
                new GetRestaurantStatusRequest(created!.RestaurantId),
                CancellationToken);

            // Assert
            response.Message.RestaurantId.Should().Be(created.RestaurantId);
            response.Message.Exists.Should().BeTrue();
            response.Message.Status.Should().Be(RestaurantStatus.Draft);
        }
        finally
        {
            await harness.Stop(CancellationToken);
        }
    }

    [Fact]
    public async Task GetRestaurantStatusRequest_Returns_ExistsFalse_And_UnknownStatus_ForMissingRestaurant()
    {
        var harness = _factory.Services.GetRequiredService<ITestHarness>();
        await harness.Start();
        try
        {
            var restaurantId = Guid.NewGuid();

            var response = await harness.Bus.Request<GetRestaurantStatusRequest, GetRestaurantStatusResponse>(
                new GetRestaurantStatusRequest(restaurantId),
                CancellationToken);

            response.Message.RestaurantId.Should().Be(restaurantId);
            response.Message.Exists.Should().BeFalse();
            response.Message.Status.Should().Be(RestaurantStatus.Unknown);
        }
        finally
        {
            await harness.Stop(CancellationToken);
        }
    }
}
