using FluentAssertions;

using RestoRate.Contracts.Restaurant.DTOs;
using RestoRate.Contracts.Restaurant.DTOs.CRUD;
using RestoRate.RestaurantService.IntegrationTests.Helpers;

using ContractRestaurantStatus = RestoRate.Contracts.Restaurant.RestaurantStatus;
using DomainRestaurantStatus = RestoRate.SharedKernel.Enums.RestaurantStatus;

namespace RestoRate.RestaurantService.IntegrationTests.Api;

public class RestaurantApiTests : IClassFixture<RestaurantWebApplicationFactory>
// , IAsyncLifetime
{
    private readonly ITestContextAccessor _testContextAccessor;
    private readonly RestaurantWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    private readonly List<Guid> _createdRestaurantIds = new();
    private CancellationToken CancellationToken => _testContextAccessor.Current.CancellationToken;

    public RestaurantApiTests(
        RestaurantWebApplicationFactory factory,
        ITestOutputHelper output,
        ITestContextAccessor testContextAccessor)
    {
        _factory = factory;
        _output = output;
        _testContextAccessor = testContextAccessor;

        _client = _factory.CreateClientWithUser(TestUser.User);
    }

    [Fact]
    public async Task CreateRestaurant_ValidData_ReturnsCreatedRestaurant()
    {
        // Arrange
        var request = RestaurantTestData.CreateValidRestaurantRequest("Ресторан для тестирования интеграции");

        // Act
        var response = await _client.PostAsJsonAsync("/restaurants", request, CancellationToken);
        var content = await response.Content.ReadAsStringAsync(CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created,
            $"Expected 201 but got {response.StatusCode}. Response: {content}");

        var restaurant = await response.Content.ReadFromJsonAsync<RestaurantDto>(CancellationToken);
        restaurant.Should().NotBeNull();
        restaurant!.RestaurantId.Should().NotBeEmpty();
        restaurant.Name.Should().Be(request.Name);
        restaurant.RestaurantStatus.Should().Be(DomainRestaurantStatus.Draft.Name);

        _createdRestaurantIds.Add(restaurant.RestaurantId);
        _output.WriteLine($"Созданный ресторан с ID: {restaurant.RestaurantId}");
    }

    [Fact]
    public async Task GetRestaurantById_ExistingRestaurant_ReturnsRestaurant()
    {
        // Arrange - создаем ресторан
        var createRequest = RestaurantTestData.CreateValidRestaurantRequest();
        var createResponse = await _client.PostAsJsonAsync("/restaurants", createRequest, CancellationToken);
        var createdRestaurant = await createResponse.Content.ReadFromJsonAsync<RestaurantDto>(CancellationToken);
        _createdRestaurantIds.Add(createdRestaurant!.RestaurantId);

        // Act
        var response = await _client.GetAsync($"/restaurants/{createdRestaurant.RestaurantId}", CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var restaurant = await response.Content.ReadFromJsonAsync<RestaurantDto>(CancellationToken);
        restaurant.Should().NotBeNull();
        restaurant!.RestaurantId.Should().Be(createdRestaurant.RestaurantId);
        restaurant.Name.Should().Be(createRequest.Name);
    }

    [Fact]
    public async Task GetRestaurantById_NonExistingRestaurant_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/restaurants/{nonExistingId}", CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateRestaurant_ValidData_ReturnsNoContent()
    {
        // Arrange - создаем ресторан
        var createRequest = RestaurantTestData.CreateValidRestaurantRequest("Гурман Бар");
        var createResponse = await _client.PostAsJsonAsync("/restaurants", createRequest, CancellationToken);
        var createdRestaurant = await createResponse.Content.ReadFromJsonAsync<RestaurantDto>(CancellationToken);
        _createdRestaurantIds.Add(createdRestaurant!.RestaurantId);

        var updateRequest = RestaurantTestData.CreateValidUpdateRequest(
            createdRestaurant.RestaurantId,
            "Муссон");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/restaurants/{createdRestaurant.RestaurantId}",
            updateRequest,
            CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Проверяем что данные обновились
        var getResponse = await _client.GetAsync($"/restaurants/{createdRestaurant.RestaurantId}", CancellationToken);
        var updatedRestaurant = await getResponse.Content.ReadFromJsonAsync<RestaurantDto>(CancellationToken);

        updatedRestaurant.Should().NotBeNull();
        updatedRestaurant!.Name.Should().Be("Муссон");
    }

    [Fact]
    public async Task DeleteRestaurant_ExistingRestaurant_ReturnsNoContent()
    {
        // Arrange
        var createRequest = RestaurantTestData.CreateValidRestaurantRequest();
        var createResponse = await _client.PostAsJsonAsync("/restaurants", createRequest, CancellationToken);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdRestaurant = await createResponse.Content.ReadFromJsonAsync<RestaurantDto>(CancellationToken);
        createdRestaurant.Should().NotBeNull();

        // Act
        var deleteResponse = await _client.DeleteAsync($"/restaurants/{createdRestaurant!.RestaurantId}", CancellationToken);
        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Проверяем удаление
        var getResponse = await _client.GetAsync($"/restaurants/{createdRestaurant.RestaurantId}", CancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var archivedRestaurant = await getResponse.Content.ReadFromJsonAsync<RestaurantDto>(CancellationToken);
        archivedRestaurant.Should().NotBeNull();
        archivedRestaurant!.RestaurantStatus.Should().Be(DomainRestaurantStatus.Archived.Name);
    }

    [Fact]
    public async Task ModerateRestaurant_ExistingRestaurant_ValidTransition_ReturnsNoContent()
    {
        // Arrange - create restaurant
        var createRequest = RestaurantTestData.CreateValidRestaurantRequest();
        var createResponse = await _client.PostAsJsonAsync("/restaurants", createRequest, CancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<RestaurantDto>(CancellationToken);
        created.Should().NotBeNull();

        var dto = new ModerationRestaurantDto
        {
            Status = ContractRestaurantStatus.OnModeration,
            Reason = null
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/restaurants/moderate/{created!.RestaurantId}", dto, CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/restaurants/{created.RestaurantId}", CancellationToken);
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await getResponse.Content.ReadFromJsonAsync<RestaurantDto>(CancellationToken);
        updated!.RestaurantStatus.Should().Be(DomainRestaurantStatus.OnModeration.Name);
    }

    [Fact]
    public async Task ModerateRestaurant_InvalidTransition_ReturnsUnprocessable()
    {
        // Arrange - create restaurant
        var createRequest = RestaurantTestData.CreateValidRestaurantRequest();
        var createResponse = await _client.PostAsJsonAsync("/restaurants", createRequest, CancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<RestaurantDto>(CancellationToken);
        created.Should().NotBeNull();

        var dto = new ModerationRestaurantDto
        {
            Status = ContractRestaurantStatus.Published, // invalid from Draft
            Reason = null
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/restaurants/moderate/{created!.RestaurantId}", dto, CancellationToken);

        // Assert
        response.StatusCode.Should().Be((HttpStatusCode)StatusCodes.Status422UnprocessableEntity);
    }

    [Fact]
    public async Task ModerateRestaurant_NonExistingRestaurant_ReturnsNotFound()
    {
        var dto = new ModerationRestaurantDto
        {
            Status = ContractRestaurantStatus.OnModeration,
            Reason = null
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"/restaurants/moderate/{Guid.NewGuid()}", dto, CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllRestaurants_ReturnsPagedList()
    {
        // Arrange - создаем несколько ресторанов
        for (int i = 0; i < 3; i++)
        {
            var request = RestaurantTestData.CreateValidRestaurantRequest($"Restaurant {i}");
            var createResponse = await _client.PostAsJsonAsync("/restaurants", request, CancellationToken);
            var restaurant = await createResponse.Content.ReadFromJsonAsync<RestaurantDto>(CancellationToken);
            _createdRestaurantIds.Add(restaurant!.RestaurantId);
        }

        // Act
        var getAllRestoResponse = await _client.GetAsync("/restaurants?pageNumber=1&pageSize=10", CancellationToken);

        // Assert
        getAllRestoResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await getAllRestoResponse.Content.ReadFromJsonAsync<PagedResultDto>(CancellationToken);
        result.Should().NotBeNull();
        result!.Items.Should().HaveCountGreaterThanOrEqualTo(3);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task CreateRestaurant_WithImages_ReturnsRestaurantWithoutImages()
    {
        // Arrange
        var request = RestaurantTestData.CreateValidRestaurantRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/restaurants", request, CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var restaurant = await response.Content.ReadFromJsonAsync<RestaurantDto>(CancellationToken);
        restaurant.Should().NotBeNull();
        restaurant!.Images.Should().BeEmpty(); // По CQRS изображения не возвращаются при Create

        _createdRestaurantIds.Add(restaurant.RestaurantId);
    }

    sealed private record PagedResultDto
    {
        public List<RestaurantDto> Items { get; init; } = new();
        public int TotalCount { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
    }
}
