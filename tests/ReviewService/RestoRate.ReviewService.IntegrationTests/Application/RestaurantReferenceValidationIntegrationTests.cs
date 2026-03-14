using FluentAssertions;

using MassTransit.Testing;

using Mediator;

using RestoRate.Contracts.Restaurant;
using RestoRate.ReviewService.Application.UseCases.RestaurantReferences.RestaurantReferenceValidation;
using RestoRate.ReviewService.Domain.RestaurantReferenceAggregate;
using RestoRate.Testing.Common.Helpers;

namespace RestoRate.ReviewService.IntegrationTests.Application;

public sealed class RestaurantReferenceValidationIntegrationTests : IClassFixture<ReviewWebApplicationFactory>
{
    private readonly ReviewWebApplicationFactory _factory;
    private readonly ITestContextAccessor _testContextAccessor;

    private CancellationToken CancellationToken => _testContextAccessor.Current.CancellationToken;

    public RestaurantReferenceValidationIntegrationTests(
        ReviewWebApplicationFactory factory,
        ITestContextAccessor testContextAccessor)
    {
        _factory = factory;
        _testContextAccessor = testContextAccessor;
    }

    [Fact]
    public async Task ValidateRestaurantReference_WhenProjectionIsStale_RefreshesProjectionFromRemoteStatus()
    {
        var harness = _factory.Services.GetRequiredService<ITestHarness>();
        var responder = _factory.Services.GetRequiredService<TestRestaurantStatusResponder>();
        responder.Reset();

        await harness.Start();
        try
        {
            var restaurantId = Guid.NewGuid();
            responder.SetResponse(restaurantId, exists: true, status: RestaurantStatus.Published);

            await using (var seedScope = _factory.Services.CreateAsyncScope())
            {
                var repository = seedScope.ServiceProvider.GetRequiredService<Ardalis.SharedKernel.IRepository<RestaurantReference>>();

                await repository.AddAsync(
                    RestaurantReference.Create(
                        restaurantId,
                        SharedKernel.Enums.RestaurantStatus.OnModeration,
                        DateTime.UtcNow.AddMinutes(-10)),
                    CancellationToken);
            }

            await using (var executionScope = _factory.Services.CreateAsyncScope())
            {
                var sender = executionScope.ServiceProvider.GetRequiredService<ISender>();

                var result = await sender.Send(
                    new RestaurantReferenceValidationCommand(restaurantId),
                    CancellationToken);

                result.IsSuccess.Should().BeTrue();
                result.Value.Should().BeTrue();
            }

            await Eventually.SucceedsAsync(async () =>
            {
                await using var assertScope = _factory.Services.CreateAsyncScope();
                var repository = assertScope.ServiceProvider.GetRequiredService<Ardalis.SharedKernel.IRepository<RestaurantReference>>();

                var restaurantReference = await repository.GetByIdAsync(restaurantId, CancellationToken);
                restaurantReference.Should().NotBeNull();
                restaurantReference!.RestaurantStatus.Should().Be(SharedKernel.Enums.RestaurantStatus.Published);
                restaurantReference.LastSynchronizedAt.Should().NotBeNull();
                restaurantReference.LastSynchronizedAt.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
            }, timeout: TimeSpan.FromSeconds(3));

            responder.RequestedRestaurantIds().Should().ContainSingle(id => id == restaurantId);
        }
        finally
        {
            responder.Reset();
            await harness.Stop(CancellationToken);
        }
    }
}
