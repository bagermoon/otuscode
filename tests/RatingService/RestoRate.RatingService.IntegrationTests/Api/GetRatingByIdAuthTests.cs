using FluentAssertions;

using Microsoft.AspNetCore.Mvc.Testing;

namespace RestoRate.RatingService.IntegrationTests.Api;

[Collection("RatingService collection")]
public sealed class GetRatingByIdAuthTests : IClassFixture<RatingWebApplicationFactory>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly ITestContextAccessor _testContextAccessor;
    private CancellationToken CancellationToken => _testContextAccessor.Current.CancellationToken;

    public GetRatingByIdAuthTests(RatingWebApplicationFactory factory, ITestContextAccessor testContextAccessor)
    {
        _factory = factory;
        _testContextAccessor = testContextAccessor;
    }

    [Fact]
    public async Task Anonymous_GetRatingById_Returns401()
    {
        using var client = _factory.CreateClientWithUser(TestUser.Anonymous);

        var response = await client.GetAsync($"/ratings/{Guid.NewGuid()}", CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task NonAdmin_GetRatingById_Returns403()
    {
        using var client = _factory.CreateClientWithUser(TestUser.User);

        var response = await client.GetAsync($"/ratings/{Guid.NewGuid()}", CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_GetRatingById_Returns200()
    {
        using var client = _factory.CreateClientWithUser(TestUser.Admin);

        var response = await client.GetAsync($"/ratings/{Guid.NewGuid()}", CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
