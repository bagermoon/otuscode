using FluentAssertions;

using RestoRate.Review.Application.DTOs;

namespace RestoRate.Review.IntegrationTests.Api;

public class GetReviewByIdTests : IClassFixture<ReviewWebApplicationFactory>
{
    private readonly ReviewWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestContextAccessor _testContextAccessor;
    private CancellationToken CancellationToken => _testContextAccessor.Current.CancellationToken;

    public GetReviewByIdTests(
        ReviewWebApplicationFactory factory,
        ITestContextAccessor testContextAccessor)
    {
        _factory = factory;
        _client = _factory.CreateClientWithUser(TestUser.User);
        _testContextAccessor = testContextAccessor;
    }

    [Fact]
    public async Task GetReviewById_ExistingReview_ReturnsReview()
    {
        // Arrange - create a review first
        var createDto = new CreateReviewDto(Guid.NewGuid(), Guid.NewGuid(), 5, "Integration test review");
        var createResponse = await _client.PostAsJsonAsync("/reviews/", createDto, CancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<ReviewDto>(CancellationToken);
        created.Should().NotBeNull();

        // Act
        var response = await _client.GetAsync($"/reviews/{created!.Id}", CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await response.Content.ReadFromJsonAsync<ReviewDto>(CancellationToken);
        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(created.Id);
        fetched.Text.Should().Be(createDto.Text);
    }

    [Fact]
    public async Task GetReviewById_NonExistingReview_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/reviews/{nonExistingId}", CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
