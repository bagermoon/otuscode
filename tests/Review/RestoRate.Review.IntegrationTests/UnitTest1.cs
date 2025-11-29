
using FluentAssertions;
using RestoRate.Review.Application.DTOs;

namespace RestoRate.Review.IntegrationTests;

public class UnitTest1 : IClassFixture<ReviewWebApplicationFactory>
{
    private readonly ReviewWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly ITestContextAccessor _testContextAccessor;
    private CancellationToken CancellationToken => _testContextAccessor.Current.CancellationToken;

    public UnitTest1(
        ReviewWebApplicationFactory factory,
        ITestContextAccessor testContextAccessor)
    {
        _factory = factory;
        _client = _factory.CreateClientWithUser(TestUser.User);
        _testContextAccessor = testContextAccessor;
    }

    [Fact]
    public async Task Test_Review_Is_Created()
    {
        CreateReviewDto review = new (RestaurantId:  Guid.NewGuid(), UserId: Guid.NewGuid(), Rating: 5, Text: "Hello");
        var response = await _client.PostAsJsonAsync("/reviews/", review, CancellationToken);
        var content = await response.Content.ReadAsStringAsync(CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created,
            $"Expected 201 but got {response.StatusCode}. Response: {content}");

        Assert.True(true);
    }
}
