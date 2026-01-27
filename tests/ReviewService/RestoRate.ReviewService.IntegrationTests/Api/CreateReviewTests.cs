using FluentAssertions;

using RestoRate.Contracts.Common.Dtos;
using RestoRate.Contracts.Review.Dtos;

namespace RestoRate.ReviewService.IntegrationTests.Api;

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
        var createDto = new CreateReviewDto(Guid.NewGuid(), Guid.NewGuid(), 4m, null, "Integration create review");
        var response = await _client.PostAsJsonAsync("/reviews/", createDto, CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<ReviewDto>(CancellationToken);
        created.Should().NotBeNull();
        created!.Id.Should().NotBeEmpty();
        created.Comment.Should().Be(createDto.Comment);
        created.Rating.Should().Be(createDto.Rating);
        created.AverageCheck.Should().Be(createDto.AverageCheck);
    }

    [Fact]
    public async Task CreateReview_WithAverageCheck_PreservesAverageCheck()
    {
        var averageCheck = new MoneyDto(1234.56m, "RUB");
        var createDto = new CreateReviewDto(Guid.NewGuid(), Guid.NewGuid(), 4.5m, averageCheck, "Integration create review with average check");

        var response = await _client.PostAsJsonAsync("/reviews/", createDto, CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<ReviewDto>(CancellationToken);
        created.Should().NotBeNull();
        created!.AverageCheck.Should().Be(averageCheck);
    }
}
