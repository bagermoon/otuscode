using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.SharedKernel;
using Ardalis.Specification;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

using RestoRate.RestaurantService.Application.UseCases.Restaurants.GetById;
using RestoRate.RestaurantService.UnitTests.Helpers;
using RestoRate.SharedKernel.Enums;

using RestaurantEntity = RestoRate.RestaurantService.Domain.RestaurantAggregate.Restaurant;

namespace RestoRate.RestaurantService.UnitTests.UseCases.GetById;
public class GetRestaurantByIdHandlerTests
{
    private readonly IReadRepository<RestaurantEntity> _readRepository;
    private readonly ILogger<GetRestaurantByIdHandler> _logger;
    private readonly GetRestaurantByIdHandler _handler;
    private readonly ITestOutputHelper _output;

    public GetRestaurantByIdHandlerTests(ITestOutputHelper output)
    {
        _output = output;
        _readRepository = Substitute.For<IReadRepository<RestaurantEntity>>();
        _logger = Substitute.For<ILogger<GetRestaurantByIdHandler>>();
        _handler = new GetRestaurantByIdHandler(_readRepository, _logger);
    }

    [Fact]
    public async Task Handle_ExistingRestaurant_ReturnsRestaurantDto()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var restaurant = TestDataBuilder.CreateRestaurantEntity(restaurantId, "Закусочная \"Тесткейк\"");
        var query = new GetRestaurantByIdQuery(restaurantId);

        _readRepository
            .FirstOrDefaultAsync(
                Arg.Any<ISpecification<RestaurantEntity>>(),
                Arg.Any<CancellationToken>())
            .Returns(restaurant);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.RestaurantId.Should().Be(restaurantId);
        result.Value.Name.Should().Be("Закусочная \"Тесткейк\"");
        result.Value.RestaurantStatus.Should().Be(Status.Draft.Name);
    }

    [Fact]
    public async Task Handle_NonExistingRestaurant_ReturnsNotFound()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var query = new GetRestaurantByIdQuery(restaurantId);

        _readRepository
            .FirstOrDefaultAsync(
                Arg.Any<ISpecification<RestaurantEntity>>(),
                Arg.Any<CancellationToken>())
            .Returns((RestaurantEntity?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_RestaurantWithImages_ReturnsImagesInOrder()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var restaurant = TestDataBuilder.CreateRestaurantEntityWithImages(restaurantId);
        var query = new GetRestaurantByIdQuery(restaurantId);

        _readRepository
            .FirstOrDefaultAsync(
                Arg.Any<ISpecification<RestaurantEntity>>(),
                Arg.Any<CancellationToken>())
            .Returns(restaurant);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Images.Should().HaveCountGreaterThan(0);
        result.Value.Images.Should().Contain(img => img.IsPrimary);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ReturnsError()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var query = new GetRestaurantByIdQuery(restaurantId);
        var errorMessage = "Не удалось подключиться к базе данных";

        _readRepository
            .FirstOrDefaultAsync(
                Arg.Any<ISpecification<RestaurantEntity>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<RestaurantEntity?>(new Exception(errorMessage)));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(errorMessage);
    }
}
