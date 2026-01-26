using System;
using System.Threading;
using System.Threading.Tasks;

using AutoFixture;
using AutoFixture.AutoNSubstitute;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using NSubstitute;

using RestoRate.Contracts.Restaurant;
using RestoRate.ReviewService.Application.UseCases.RestaurantReferences.UpsertRestaurant;
using RestoRate.ReviewService.Domain.RestaurantReferenceAggregate;

using Xunit;

namespace RestoRate.ReviewService.UnitTests.UseCases.RestaurantReferences;

public class UpsertRestaurantHandlerTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());

    [Fact]
    public async Task Handle_WhenReferenceExistsAndSameStatus_ReturnsStored_AndDoesNotUpdate()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var existing = RestaurantReference.Create(restaurantId, SharedKernel.Enums.RestaurantStatus.Draft);

        var repo = _fixture.Freeze<Ardalis.SharedKernel.IRepository<RestaurantReference>>();
        repo.GetByIdAsync(restaurantId, Arg.Any<CancellationToken>()).Returns(existing);

        var logger = _fixture.Freeze<ILogger<UpsertRestaurantHandler>>();
        var handler = new UpsertRestaurantHandler(repo, logger);

        var cmd = new UpsertRestaurantCommand(restaurantId, RestaurantStatus.Draft);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(RestaurantStatus.Draft);
        _ = repo.DidNotReceive().UpdateAsync(Arg.Any<RestaurantReference>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenReferenceExistsAndDifferentStatus_UpdatesAndReturnsNewStatus()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var existing = RestaurantReference.Create(restaurantId, SharedKernel.Enums.RestaurantStatus.Draft);

        var repo = _fixture.Freeze<Ardalis.SharedKernel.IRepository<RestaurantReference>>();
        repo.GetByIdAsync(restaurantId, Arg.Any<CancellationToken>()).Returns(existing);
        repo.UpdateAsync(Arg.Any<RestaurantReference>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));

        var logger = _fixture.Freeze<ILogger<UpsertRestaurantHandler>>();
        var handler = new UpsertRestaurantHandler(repo, logger);

        var cmd = new UpsertRestaurantCommand(restaurantId, RestaurantStatus.Published);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(RestaurantStatus.Published);
        _ = repo.Received(1).UpdateAsync(Arg.Any<RestaurantReference>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenInsertRacesWithDuplicateKey_ReReadsExistingAndReturnsStored()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var existing = RestaurantReference.Create(restaurantId, SharedKernel.Enums.RestaurantStatus.OnModeration);

        var repo = _fixture.Freeze<Ardalis.SharedKernel.IRepository<RestaurantReference>>();

        // First read: not found. Second read: found (other writer inserted).
        repo.GetByIdAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns((RestaurantReference?)null, existing);

        repo.AddAsync(Arg.Any<RestaurantReference>(), Arg.Any<CancellationToken>())
            .Returns<Task<RestaurantReference>>(_ => throw new InvalidOperationException("E11000 duplicate key error collection"));

        var logger = _fixture.Freeze<ILogger<UpsertRestaurantHandler>>();
        var handler = new UpsertRestaurantHandler(repo, logger);

        var cmd = new UpsertRestaurantCommand(restaurantId, RestaurantStatus.Published);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(RestaurantStatus.OnModeration);
    }
}
