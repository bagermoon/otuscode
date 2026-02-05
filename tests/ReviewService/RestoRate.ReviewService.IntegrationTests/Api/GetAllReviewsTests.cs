using FluentAssertions;

using Microsoft.AspNetCore.WebUtilities;

using RestoRate.Contracts.Common;
using RestoRate.Contracts.Common.Dtos;
using RestoRate.Contracts.Review.Dtos;
using RestoRate.ReviewService.Domain.UserReferenceAggregate;
using RestoRate.Testing.Common.Auth;
using RestoRate.Testing.Common.Helpers;

namespace RestoRate.ReviewService.IntegrationTests.Api;

public class GetAllReviewsTests : IClassFixture<ReviewWebApplicationFactory>
{
    private readonly ReviewWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestContextAccessor _testContextAccessor;
    private CancellationToken CancellationToken => _testContextAccessor.Current.CancellationToken;

    public GetAllReviewsTests(
        ReviewWebApplicationFactory factory,
        ITestContextAccessor testContextAccessor)
    {
        _factory = factory;
        _client = _factory.CreateClientWithUser(TestUser.User);
        _testContextAccessor = testContextAccessor;
    }

    [Fact]
    public async Task GetAllReviews_ReturnsPagedList()
    {
        // Arrange
        var userId = TestUsers.Get(TestUser.User).UserId;
        for (int i = 0; i < 3; i++)
        {
            var create = new CreateReviewDto(
                RestaurantId: Guid.NewGuid(),
                UserId: userId,
                Rating: 4.0m + (i * 0.1m),
                AverageCheck: new MoneyDto(1000m + (i * 100m), "RUB"),
                Comment: $"comment {i}");

            var createResponse = await _client.PostAsJsonAsync("/reviews", create, CancellationToken);
            createResponse.EnsureSuccessStatusCode();
        }

        // Act
        var getAllResponse = await _client.GetAsync("/reviews?pageNumber=1&pageSize=10", CancellationToken);

        // Assert
        getAllResponse.EnsureSuccessStatusCode();

        var result = await getAllResponse.Content.ReadFromJsonAsync<PagedResult<ReviewDto>>(CancellationToken);

        result.Should().NotBeNull();
        result!.Items.Should().HaveCountGreaterThanOrEqualTo(3);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task GetAllReviews_WithStatusesFilter_ReturnsFilteredList()
    {
        // Arrange
        var userId = TestUsers.Get(TestUser.User).UserId;
        for (int i = 0; i < 2; i++)
        {
            var create = new CreateReviewDto(
                RestaurantId: Guid.NewGuid(),
                UserId: userId,
                Rating: 4.0m,
                AverageCheck: null,
                Comment: $"pending {i}");

            var createResponse = await _client.PostAsJsonAsync("/reviews", create, CancellationToken);
            createResponse.EnsureSuccessStatusCode();
        }

        var query = new Dictionary<string, string?>
        {
            ["pageNumber"] = "1",
            ["pageSize"] = "10",
        };

        var url = QueryHelpers.AddQueryString("/reviews", query);
        url += "&statuses=Pending&statuses=Approved";

        // Act
        var response = await _client.GetAsync(url, CancellationToken);

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ReviewDto>>(CancellationToken);

        result.Should().NotBeNull();
        result!.Items.Should().OnlyContain(r => r.Id != Guid.Empty);
        // All created reviews are Pending by default, so this should include at least those.
        result.Items.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAllReviews_ForAuthenticatedUser_FillsUserReferenceInEachCreatedItem()
    {
        var userInfo = TestUsers.Get(TestUser.User);
        var createdIds = new List<Guid>();

        for (var i = 0; i < 3; i++)
        {
            var create = new CreateReviewDto(
                RestaurantId: Guid.NewGuid(),
                UserId: userInfo.UserId,
                Rating: 4.0m,
                AverageCheck: null,
                Comment: $"userRef propagation {i}");

            var createResponse = await _client.PostAsJsonAsync("/reviews", create, CancellationToken);
            createResponse.EnsureSuccessStatusCode();

            var created = await createResponse.Content.ReadFromJsonAsync<ReviewDto>(CancellationToken);
            created.Should().NotBeNull();
            createdIds.Add(created!.Id);
        }

        await Eventually.SucceedsAsync(async () =>
        {
            await using var scope = _factory.Services.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<Ardalis.SharedKernel.IRepository<UserReference>>();
            var userRef = await repo.GetByIdAsync(userInfo.UserId, CancellationToken);
            userRef.Should().NotBeNull();
            userRef!.Name.Should().Be(userInfo.Name);
        }, timeout: TimeSpan.FromSeconds(2));

        await Eventually.SucceedsAsync(async () =>
        {
            var getAllResponse = await _client.GetAsync("/reviews?pageNumber=1&pageSize=50", CancellationToken);
            getAllResponse.EnsureSuccessStatusCode();

            var result = await getAllResponse.Content.ReadFromJsonAsync<PagedResult<ReviewDto>>(CancellationToken);
            result.Should().NotBeNull();

            var createdReviews = result!.Items.Where(r => createdIds.Contains(r.Id)).ToList();
            createdReviews.Should().HaveCount(createdIds.Count);
            createdReviews.Should().OnlyContain(r => r.User != null);
            createdReviews.Select(r => r.User!.Name).Should().OnlyContain(n => n == userInfo.Name);
        }, timeout: TimeSpan.FromSeconds(3));
    }
}
