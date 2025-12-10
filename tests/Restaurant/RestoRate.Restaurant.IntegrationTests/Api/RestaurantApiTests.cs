using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using RestoRate.Restaurant.Application.DTOs;
using RestoRate.Restaurant.IntegrationTests.Helpers;
using RestoRate.SharedKernel.Enums;

namespace RestoRate.Restaurant.IntegrationTests.Api;

[Collection("AspireAppHost collection")]
public class RestaurantApiTests : IAsyncLifetime
{
    private readonly AspireAppHost _appHost;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly List<Guid> _createdRestaurantIds = new();

    public RestaurantApiTests(
        AspireAppHost appHost,
        ITestOutputHelper output)
    {
        _appHost = appHost;
        _output = output;
        _client = _appHost.CreateRestaurantApiClient();
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        foreach (var id in _createdRestaurantIds)
        {
            try
            {
                await _client.DeleteAsync($"/restaurants/{id}");
            }
            catch{ } // Игнорируем ошибки при очистке
        }
    }

    [Fact]
    public async Task CreateRestaurant_ValidData_ReturnsCreatedRestaurant()
    {
        // Arrange
        var request = RestaurantTestData.CreateValidRestaurantRequest("Ресторан для тестирования интеграции");

        // Act
        var response = await _client.PostAsJsonAsync("/restaurants", request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created,
            $"Expected 201 but got {response.StatusCode}. Response: {content}");

        var restaurant = await response.Content.ReadFromJsonAsync<RestaurantDto>();
        restaurant.Should().NotBeNull();
        restaurant!.RestaurantId.Should().NotBeEmpty();
        restaurant.Name.Should().Be(request.Name);
        restaurant.RestaurantStatus.Should().Be(Status.Draft.Name);

        _createdRestaurantIds.Add(restaurant.RestaurantId);
        _output.WriteLine($"Созданный ресторан с ID: {restaurant.RestaurantId}");
    }

    [Fact]
    public async Task GetRestaurantById_ExistingRestaurant_ReturnsRestaurant()
    {
        // Arrange - создаем ресторан
        var createRequest = RestaurantTestData.CreateValidRestaurantRequest();
        var createResponse = await _client.PostAsJsonAsync("/restaurants", createRequest);
        var createdRestaurant = await createResponse.Content.ReadFromJsonAsync<RestaurantDto>();
        _createdRestaurantIds.Add(createdRestaurant!.RestaurantId);

        // Act
        var response = await _client.GetAsync($"/restaurants/{createdRestaurant.RestaurantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var restaurant = await response.Content.ReadFromJsonAsync<RestaurantDto>();
        restaurant.Should().NotBeNull();
        restaurant!.RestaurantId.Should().Be(createdRestaurant.RestaurantId);
        restaurant.Name.Should().Be(createRequest.Name);
        restaurant.RestaurantStatus.Should().Be(createdRestaurant.RestaurantStatus);
    }

    [Fact]
    public async Task GetRestaurantById_NonExistingRestaurant_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/restaurants/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateRestaurant_ValidData_ReturnsNoContent()
    {
        // Arrange - создаем ресторан
        var createRequest = RestaurantTestData.CreateValidRestaurantRequest("Гурман Бар");
        var createResponse = await _client.PostAsJsonAsync("/restaurants", createRequest);
        var createdRestaurant = await createResponse.Content.ReadFromJsonAsync<RestaurantDto>();
        _createdRestaurantIds.Add(createdRestaurant!.RestaurantId);

        var updateRequest = RestaurantTestData.CreateValidUpdateRequest(
            createdRestaurant.RestaurantId,
            "Муссон");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/restaurants/{createdRestaurant.RestaurantId}",
            updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Проверяем что данные обновились
        var getResponse = await _client.GetAsync($"/restaurants/{createdRestaurant.RestaurantId}");
        var updatedRestaurant = await getResponse.Content.ReadFromJsonAsync<RestaurantDto>();

        updatedRestaurant.Should().NotBeNull();
        updatedRestaurant!.Name.Should().Be("Муссон");
        updatedRestaurant.Description.Should().Be("Муссон обновился");
    }

    [Fact]
    public async Task DeleteRestaurant_ExistingRestaurant_ReturnsNoContent()
    {
        // Arrange
        var createRequest = RestaurantTestData.CreateValidRestaurantRequest();
        var createResponse = await _client.PostAsJsonAsync("/restaurants", createRequest);
        var createdRestaurant = await createResponse.Content.ReadFromJsonAsync<RestaurantDto>();
        _createdRestaurantIds.Add(createdRestaurant!.RestaurantId);

        // Act - Удаляем (Soft Delete)
        var deleteResponse = await _client.DeleteAsync($"/restaurants/{createdRestaurant.RestaurantId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act
        var getResponse = await _client.GetAsync($"/restaurants/{createdRestaurant.RestaurantId}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var archivedRestaurant = await getResponse.Content.ReadFromJsonAsync<RestaurantDto>();
        archivedRestaurant.Should().NotBeNull();
        archivedRestaurant!.RestaurantStatus.Should().Be(Status.Archived.Name);
    }

    [Fact]
    public async Task GetAllRestaurants_ReturnsPagedList()
    {
        // Arrange - создаем несколько ресторанов
        for (int i = 0; i < 3; i++)
        {
            var request = RestaurantTestData.CreateValidRestaurantRequest($"Restaurant {i}");
            var createResponse = await _client.PostAsJsonAsync("/restaurants", request);
            var restaurant = await createResponse.Content.ReadFromJsonAsync<RestaurantDto>();
            _createdRestaurantIds.Add(restaurant!.RestaurantId);
        }

        // Act
        var getAllRestoResponse = await _client.GetAsync("/restaurants?pageNumber=1&pageSize=10");

        // Assert
        getAllRestoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await getAllRestoResponse.Content.ReadFromJsonAsync<PagedResultDto>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCountGreaterOrEqualTo(3);
        result.TotalCount.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public async Task CreateRestaurant_WithImages_ReturnsRestaurantWithoutImages()
    {
        // Arrange
        var request = RestaurantTestData.CreateValidRestaurantRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/restaurants", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var restaurant = await response.Content.ReadFromJsonAsync<RestaurantDto>();
        restaurant.Should().NotBeNull();
        restaurant!.Images.Should().BeEmpty(); // По CQRS изображения не возвращаются при Create

        _createdRestaurantIds.Add(restaurant.RestaurantId);
    }

    private record PagedResultDto
    {
        public List<RestaurantDto> Items { get; init; } = new();
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
    }
}
