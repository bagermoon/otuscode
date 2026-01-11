using System.Diagnostics;

using FluentAssertions;

using MassTransit;
using MassTransit.Testing;

using RestoRate.Contracts.Common.Dtos;
using RestoRate.Contracts.Restaurant.DTOs;
using RestoRate.Contracts.Rating.Events;
using RestoRate.RestaurantService.IntegrationTests.Helpers;

using Microsoft.AspNetCore.Mvc.Testing;

namespace RestoRate.RestaurantService.IntegrationTests.Api;

public class RestaurantRatingTests : IClassFixture<RestaurantWebApplicationFactory>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly ITestContextAccessor _testContextAccessor;
    private CancellationToken CancellationToken => _testContextAccessor.Current.CancellationToken;

    public RestaurantRatingTests(
        RestaurantWebApplicationFactory factory,
        ITestContextAccessor testContextAccessor)
    {
        (_factory, _client) = factory.CreateFactoryAndClientWithUser(TestUser.User);
        _testContextAccessor = testContextAccessor;
    }

    [Fact]
    public async Task RestaurantRatingRecalculatedEvent_Updates_Projection()
    {
        var harness = _factory.Services.GetRequiredService<ITestHarness>();
        await harness.Start();
        try
        {
            var createRequest = RestaurantTestData.CreateValidRestaurantRequest("Для рейтинга");

            var createResponse = await _client.PostAsJsonAsync("/restaurants", createRequest, CancellationToken);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var created = await createResponse.Content.ReadFromJsonAsync<RestaurantDto>(CancellationToken);
            created.Should().NotBeNull();

            var restaurantId = created!.RestaurantId;

            // Prepare event payload
            var approvedAverageRating = 4.5m;
            var approvedReviewsCount = 10;
            var approvedAverageCheck = new MoneyDto(120.50m, "RUB");

            var provisionalAverageRating = 4.2m;
            var provisionalReviewsCount = 3;
            var provisionalAverageCheck = new MoneyDto(110m, "RUB");

            var evt = new RestaurantRatingRecalculatedEvent(
                restaurantId,
                approvedAverageRating,
                approvedReviewsCount,
                approvedAverageCheck,
                provisionalAverageRating,
                provisionalReviewsCount,
                provisionalAverageCheck);

            // Publish event via the test harness bus
            await harness.Bus.Publish(evt, CancellationToken);

            // Poll until projection updated
            var sw = Stopwatch.StartNew();
            RestaurantDto? dto = null;
            while (sw.Elapsed < TimeSpan.FromSeconds(5))
            {
                var getResponse = await _client.GetAsync($"/restaurants/{restaurantId}", CancellationToken);
                if (getResponse.StatusCode == HttpStatusCode.OK)
                {
                    dto = await getResponse.Content.ReadFromJsonAsync<RestaurantDto>(CancellationToken);
                    if (dto?.Rating is not null
                        && dto.Rating.AverageRating == approvedAverageRating
                        && dto.Rating.ReviewsCount == approvedReviewsCount
                        && dto.Rating.AverageCheck.Amount == approvedAverageCheck.Amount
                        && dto.Rating.ProvisionalAverageRating == provisionalAverageRating
                        && dto.Rating.ProvisionalReviewsCount == provisionalReviewsCount
                        && dto.Rating.ProvisionalAverageCheck.Amount == provisionalAverageCheck.Amount)
                    {
                        break;
                    }
                }

                await Task.Delay(200, CancellationToken);
            }

            dto.Should().NotBeNull();
            dto!.Rating.Should().NotBeNull();
            dto.Rating.AverageRating.Should().Be(approvedAverageRating);
            dto.Rating.ReviewsCount.Should().Be(approvedReviewsCount);
            dto.Rating.AverageCheck.Amount.Should().Be(approvedAverageCheck.Amount);
            dto.Rating.ProvisionalAverageRating.Should().Be(provisionalAverageRating);
            dto.Rating.ProvisionalReviewsCount.Should().Be(provisionalReviewsCount);
            dto.Rating.ProvisionalAverageCheck.Amount.Should().Be(provisionalAverageCheck.Amount);
        }
        finally
        {
            await harness.Stop(CancellationToken);
        }
    }
}
