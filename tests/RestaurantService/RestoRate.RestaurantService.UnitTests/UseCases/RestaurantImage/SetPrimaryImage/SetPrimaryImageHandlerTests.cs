using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.SharedKernel;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RestoRate.RestaurantService.Application.UseCases.RestaurantImage.SetPrimaryImage;
using RestoRate.RestaurantService.UnitTests.Helpers;
using RestaurantEntity = RestoRate.RestaurantService.Domain.RestaurantAggregate.Restaurant;

namespace RestoRate.RestaurantService.UnitTests.UseCases.RestaurantImage.SetPrimaryImage;

public sealed class SetPrimaryImageHandlerTests
{
    private readonly IRepository<RestaurantEntity> _repository;
    private readonly ILogger<SetPrimaryImageHandler> _logger;
    private readonly SetPrimaryImageHandler _handler;

    public SetPrimaryImageHandlerTests()
    {
        _repository = Substitute.For<IRepository<RestaurantEntity>>();
        _logger = Substitute.For<ILogger<SetPrimaryImageHandler>>();
        _handler = new SetPrimaryImageHandler(_repository, _logger);
    }

    [Fact]
    public async Task Handle_ValidRequest_SetsPrimaryImageSuccessfully()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var restaurant = TestDataBuilder.CreateRestaurantEntity(restaurantId, "Закусочная \"Тесткейк\"");
        restaurant.AddImage("https://example.com/image1.jpg", "Первое", 0, true);
        restaurant.AddImage("https://example.com/image2.jpg", "Второе", 1, false);

        var images = restaurant.Images.ToList();
        var newPrimaryImageId = images[1].Id;

        var command = new SetPrimaryImageCommand(restaurantId, newPrimaryImageId);

        _repository.GetByIdAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(restaurant);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).UpdateAsync(restaurant, Arg.Any<CancellationToken>());

        var updatedSecondImage = restaurant.Images.First(img => img.Id == newPrimaryImageId);
        updatedSecondImage.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NonExistingRestaurant_ReturnsNotFound()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var command = new SetPrimaryImageCommand(restaurantId, imageId);

        _repository.GetByIdAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns((RestaurantEntity?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<RestaurantEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistingImage_ReturnsNotFound()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var nonExistingImageId = Guid.NewGuid();
        var restaurant = TestDataBuilder.CreateRestaurantEntity(restaurantId, "Закусочная \"Тесткейк\"");
        restaurant.AddImage("https://example.com/image.jpg", "Фото", 0, false);

        var command = new SetPrimaryImageCommand(restaurantId, nonExistingImageId);

        _repository.GetByIdAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(restaurant);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<RestaurantEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SetPrimaryImage_UnsetsPreviousPrimary()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var restaurant = TestDataBuilder.CreateRestaurantEntity(restaurantId, "Закусочная \"Тесткейк\"");
        restaurant.AddImage("https://example.com/image1.jpg", "Старое главное", 0, true);
        restaurant.AddImage("https://example.com/image2.jpg", "Новое главное", 1, false);

        var newPrimaryImageId = restaurant.Images.Last().Id;
        var command = new SetPrimaryImageCommand(restaurantId, newPrimaryImageId);

        _repository.GetByIdAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(restaurant);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        restaurant.Images.Count(img => img.IsPrimary).Should().Be(1);
        restaurant.Images.Single(img => img.IsPrimary).Id.Should().Be(newPrimaryImageId);
    }

    [Fact]
    public async Task Handle_AlreadyPrimaryImage_RemainsSuccessful()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var restaurant = TestDataBuilder.CreateRestaurantEntity(restaurantId, "Закусочная \"Тесткейк\"");
        restaurant.AddImage("https://example.com/image.jpg", "Главное", 0, true);

        var primaryImageId = restaurant.Images.First().Id;
        var command = new SetPrimaryImageCommand(restaurantId, primaryImageId);

        _repository.GetByIdAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(restaurant);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        restaurant.Images.First().IsPrimary.Should().BeTrue();
    }
}
