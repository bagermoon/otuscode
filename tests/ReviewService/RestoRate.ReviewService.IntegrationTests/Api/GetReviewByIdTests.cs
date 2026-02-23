using FluentAssertions;

using RestoRate.Contracts.Review.Dtos;

namespace RestoRate.ReviewService.IntegrationTests.Api;

public class GetReviewByIdTests : IClassFixture<ReviewWebApplicationFactory>
{
    private readonly ReviewWebApplicationFactory _factory;
    private readonly ITestContextAccessor _testContextAccessor;
    private CancellationToken CancellationToken => _testContextAccessor.Current.CancellationToken;

    public GetReviewByIdTests(
        ReviewWebApplicationFactory factory,
        ITestContextAccessor testContextAccessor)
    {
        _factory = factory;
        _testContextAccessor = testContextAccessor;
    }

    [Fact]
    public async Task GetReviewById_ExistingReview_ReturnsReview()
    {
        using var client = _factory.CreateClientWithUser(TestUser.User);
        // Arrange - create a review first
        var userId = TestUsers.Get(TestUser.User).UserId;
        var createDto = new CreateReviewDto(Guid.NewGuid(), userId, 5m, null, "Integration test review");
        var createResponse = await client.PostAsJsonAsync("/reviews/", createDto, CancellationToken);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<ReviewDto>(CancellationToken);
        created.Should().NotBeNull();

        // Act
        var response = await client.GetAsync($"/reviews/{created!.Id}", CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await response.Content.ReadFromJsonAsync<ReviewDto>(CancellationToken);
        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(created.Id);
        fetched.Comment.Should().Be(createDto.Comment);
    }

    [Fact]
    public async Task GetReviewById_NonExistingReview_ReturnsNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        using var client = _factory.CreateClientWithUser(TestUser.User);
        var response = await client.GetAsync($"/reviews/{nonExistingId}", CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
