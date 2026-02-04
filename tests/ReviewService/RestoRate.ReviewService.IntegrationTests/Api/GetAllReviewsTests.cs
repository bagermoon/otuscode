using FluentAssertions;

using Microsoft.AspNetCore.WebUtilities;

using RestoRate.Contracts.Common;
using RestoRate.Contracts.Common.Dtos;
using RestoRate.Contracts.Review.Dtos;

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
                AverageCheck: new MoneyDto(1000 + (i * 100), "RUB"),
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
}
