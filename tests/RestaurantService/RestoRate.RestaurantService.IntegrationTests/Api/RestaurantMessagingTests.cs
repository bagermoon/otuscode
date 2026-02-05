using FluentAssertions;

using MassTransit.Testing;

using RestoRate.Contracts.Restaurant.DTOs;
using RestoRate.RestaurantService.IntegrationTests.Helpers;
using RestoRate.Contracts.Restaurant.Events;
using RestoRate.Contracts.Restaurant.DTOs.CRUD;

using ContractRestaurantStatus = RestoRate.Contracts.Restaurant.RestaurantStatus;
using DomainRestaurantStatus = RestoRate.SharedKernel.Enums.RestaurantStatus;

using Microsoft.AspNetCore.Mvc.Testing;

namespace RestoRate.RestaurantService.IntegrationTests.Api;

public class RestaurantMessagingTests : IClassFixture<RestaurantWebApplicationFactory>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestContextAccessor _testContextAccessor;
    private CancellationToken CancellationToken => _testContextAccessor.Current.CancellationToken;

    public RestaurantMessagingTests(
        RestaurantWebApplicationFactory factory,
        ITestContextAccessor testContextAccessor)
    {
        (_factory, _client) = factory.CreateFactoryAndClientWithUser(TestUser.Admin);
        _testContextAccessor = testContextAccessor;
    }

    [Fact]
    public async Task CreateRestaurant_Publishes_RestaurantCreatedEvent()
    {
        var harness = _factory.Services.GetRequiredService<ITestHarness>();
        await harness.Start();
        try
        {
            var request = RestaurantTestData.CreateValidRestaurantRequest("Ресторан событий");

            // Act
            var response = await _client.PostAsJsonAsync("/restaurants", request, CancellationToken);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await response.Content.ReadFromJsonAsync<RestaurantDto>(CancellationToken);
            created.Should().NotBeNull();

            // Ensure persisted
            var getResponse = await _client.GetAsync($"/restaurants/{created!.RestaurantId}", CancellationToken);
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Published check
            var published = await harness.Published.Any<RestaurantCreatedEvent>(CancellationToken);
            published.Should().BeTrue();

            var msg = harness.Published.Select<RestaurantCreatedEvent>(CancellationToken).First().Context.Message;
            msg.RestaurantId.Should().Be(created.RestaurantId);
        }
        finally
        {
            await harness.Stop(CancellationToken);
        }
    }

    [Fact]
    public async Task UpdateRestaurant_Publishes_RestaurantUpdatedEvent()
    {
        var harness = _factory.Services.GetRequiredService<ITestHarness>();
        await harness.Start();
        try
        {
            // Create initial restaurant
            var createRequest = RestaurantTestData.CreateValidRestaurantRequest("Для обновления");
            var createResponse = await _client.PostAsJsonAsync("/restaurants", createRequest, CancellationToken);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await createResponse.Content.ReadFromJsonAsync<RestaurantDto>(CancellationToken);
            created.Should().NotBeNull();

            // Update
            var dto = new ModerationRestaurantDto
            {
                Status = ContractRestaurantStatus.OnModeration,
                Reason = null
            };

            var updateResponse = await _client.PatchAsJsonAsync($"/restaurants/moderate/{created!.RestaurantId}", dto, CancellationToken);
            updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Ensure persisted
            var getResponse = await _client.GetAsync($"/restaurants/{created.RestaurantId}", CancellationToken);
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var restaurant = await getResponse.Content.ReadFromJsonAsync<RestaurantDto>(CancellationToken);
            restaurant!.RestaurantStatus.Should().Be(DomainRestaurantStatus.OnModeration.Name);

            // Published check
            var published = await harness.Published.Any<RestaurantUpdatedEvent>(CancellationToken);
            published.Should().BeTrue();

            var msg = harness.Published.Select<RestaurantUpdatedEvent>(CancellationToken).First().Context.Message;
            msg.RestaurantId.Should().Be(created.RestaurantId);
        }
        finally
        {
            await harness.Stop(CancellationToken);
        }
    }

    [Fact]
    public async Task DeleteRestaurant_Publishes_RestaurantArchivedEvent()
    {
        var harness = _factory.Services.GetRequiredService<ITestHarness>();
        await harness.Start();
        try
        {
            // Create
            var createRequest = RestaurantTestData.CreateValidRestaurantRequest("Для удаления");
            var createResponse = await _client.PostAsJsonAsync("/restaurants", createRequest, CancellationToken);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await createResponse.Content.ReadFromJsonAsync<RestaurantDto>(CancellationToken);
            created.Should().NotBeNull();

            // Delete
            var deleteResponse = await _client.DeleteAsync($"/restaurants/{created!.RestaurantId}", CancellationToken);
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

            // Ensure archived
            var getResponse = await _client.GetAsync($"/restaurants/{created.RestaurantId}", CancellationToken);
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Published check
            var published = await harness.Published.Any<RestaurantArchivedEvent>(CancellationToken);
            published.Should().BeTrue();

            var msg = harness.Published.Select<RestaurantArchivedEvent>(CancellationToken).First().Context.Message;
            msg.RestaurantId.Should().Be(created.RestaurantId);
        }
        finally
        {
            await harness.Stop(CancellationToken);
        }
    }
}
