using FluentAssertions;

using System.Net.Http.Json;

using RestoRate.Contracts.Rating.Dtos;

namespace RestoRate.RatingService.IntegrationTests.Api;

[Collection("RatingService collection")]
public sealed class GetRatingByIdEmptyTests : IClassFixture<RatingWebApplicationFactory>
{
    private readonly RatingWebApplicationFactory _factory;

    private readonly ITestContextAccessor _testContextAccessor;
    private CancellationToken CancellationToken => _testContextAccessor.Current.CancellationToken;

    public GetRatingByIdEmptyTests(RatingWebApplicationFactory factory, ITestContextAccessor testContextAccessor)
    {
        _factory = factory;
        _testContextAccessor = testContextAccessor;
    }

    [Fact]
    public async Task Admin_GetRatingById_WhenNoReviews_ReturnsZeroMetrics()
    {
        using var client = _factory.CreateClientWithUser(TestUser.Admin);

        var restaurantId = Guid.NewGuid();
        var response = await client.GetAsync($"/ratings/{restaurantId}", CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var dto = await response.Content.ReadFromJsonAsync<RestaurantRatingDto>(cancellationToken: CancellationToken);
        dto.Should().NotBeNull();

        dto!.RestaurantId.Should().Be(restaurantId);
        dto.ApprovedReviewsCount.Should().Be(0);
        dto.ApprovedAverageRating.Should().Be(0m);
        dto.ProvisionalReviewsCount.Should().Be(0);
        dto.ProvisionalAverageRating.Should().Be(0m);

        dto.ApprovedAverageCheck.Amount.Should().Be(0m);
        dto.ProvisionalAverageCheck.Amount.Should().Be(0m);

        dto.ApprovedAverageCheck.Currency.Should().NotBeNullOrWhiteSpace();
        dto.ProvisionalAverageCheck.Currency.Should().NotBeNullOrWhiteSpace();
    }
}
