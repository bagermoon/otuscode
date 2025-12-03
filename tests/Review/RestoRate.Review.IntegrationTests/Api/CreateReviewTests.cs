using FluentAssertions;

using RestoRate.Review.Application.DTOs;

namespace RestoRate.Review.IntegrationTests.Api;

public class CreateReviewTests : IClassFixture<ReviewWebApplicationFactory>
{
    private readonly ReviewWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestContextAccessor _testContextAccessor;
    private CancellationToken CancellationToken => _testContextAccessor.Current.CancellationToken;

    public CreateReviewTests(
        ReviewWebApplicationFactory factory,
        ITestContextAccessor testContextAccessor)
    {
        _factory = factory;
        _client = _factory.CreateClientWithUser(TestUser.User);
        _testContextAccessor = testContextAccessor;
    }

    [Fact]
    public async Task CreateReview_ValidData_ReturnsCreatedReview()
    {
        var createDto = new CreateReviewDto(Guid.NewGuid(), Guid.NewGuid(), 4, "Integration create review");
        var response = await _client.PostAsJsonAsync("/reviews/", createDto, CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<ReviewDto>(CancellationToken);
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Text.Should().Be(createDto.Text);
        created.Rating.Should().Be(createDto.Rating);
    }
}
