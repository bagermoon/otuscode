using FluentAssertions;

using RestoRate.Contracts.Common.Dtos;
using RestoRate.Contracts.Review.Dtos;
using RestoRate.ReviewService.Domain.UserReferenceAggregate;
using RestoRate.Testing.Common.Auth;
using RestoRate.Testing.Common.Helpers;

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
        var userId = TestUsers.Get(TestUser.User).UserId;
        var createDto = new CreateReviewDto(Guid.NewGuid(), userId, 4m, null, "Integration create review");
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
    public async Task CreateReview_UserIdMismatch_ReturnsBadRequest()
    {
        var createDto = new CreateReviewDto(Guid.NewGuid(), Guid.NewGuid(), 4m, null, "Integration create review");

        var response = await _client.PostAsJsonAsync("/reviews/", createDto, CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateReview_WithAverageCheck_PreservesAverageCheck()
    {
        var userId = TestUsers.Get(TestUser.User).UserId;
        var averageCheck = new MoneyDto(1234.56m, "RUB");
        var createDto = new CreateReviewDto(Guid.NewGuid(), userId, 4.5m, averageCheck, "Integration create review with average check");

        var response = await _client.PostAsJsonAsync("/reviews/", createDto, CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<ReviewDto>(CancellationToken);
        created.Should().NotBeNull();
        created!.AverageCheck.Should().Be(averageCheck);
    }

    [Fact]
    public async Task CreateReview_EventuallyCreatesUserReference_AndIsReturnedByGetById()
    {
        var userId = TestUsers.Get(TestUser.User).UserId;

        var createDto = new CreateReviewDto(Guid.NewGuid(), userId, 5m, null, "Integration create review with user reference");
        var response = await _client.PostAsJsonAsync("/reviews/", createDto, CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<ReviewDto>(CancellationToken);
        created.Should().NotBeNull();
        created!.UserId.Should().Be(userId);

        await Eventually.SucceedsAsync(async () =>
        {
            await using var scope = _factory.Services.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<Ardalis.SharedKernel.IRepository<UserReference>>();
            var userRef = await repo.GetByIdAsync(userId, CancellationToken);
            userRef.Should().NotBeNull();
        }, timeout: TimeSpan.FromSeconds(2));

        var getById = await _client.GetAsync($"/reviews/{created.Id}", CancellationToken);
        getById.StatusCode.Should().Be(HttpStatusCode.OK);

        var fetched = await getById.Content.ReadFromJsonAsync<ReviewDto>(CancellationToken);
        fetched.Should().NotBeNull();
        fetched!.User.Should().NotBeNull();
        fetched.User!.UserId.Should().Be(userId);
    }
}
