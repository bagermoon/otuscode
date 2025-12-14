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
using RestoRate.RestaurantService.Application.UseCases.RestaurantImage.RemoveRestaurantImage;
using RestoRate.RestaurantService.UnitTests.Helpers;
using RestaurantEntity = RestoRate.RestaurantService.Domain.RestaurantAggregate.Restaurant;

namespace RestoRate.RestaurantService.UnitTests.UseCases.RestaurantImage.RemoveRestaurantImage;

public sealed class RemoveRestaurantImageHandlerTests
{
    private readonly IRepository<RestaurantEntity> _repository;
    private readonly ILogger<RemoveRestaurantImageHandler> _logger;
    private readonly RemoveRestaurantImageHandler _handler;

    public RemoveRestaurantImageHandlerTests()
    {
        _repository = Substitute.For<IRepository<RestaurantEntity>>();
        _logger = Substitute.For<ILogger<RemoveRestaurantImageHandler>>();
        _handler = new RemoveRestaurantImageHandler(_repository, _logger);
    }

    [Fact]
    public async Task Handle_ValidRequest_RemovesImageSuccessfully()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var restaurant = TestDataBuilder.CreateRestaurantEntity(restaurantId, "Закусочная \"Тесткейк\"");
        restaurant.AddImage("https://example.com/image.jpg", "Фото", 0, false);

        // Получаем реальный ID добавленного изображения
        var actualImageId = restaurant.Images.First().Id;

        var command = new RemoveRestaurantImageCommand(restaurantId, actualImageId);

        _repository.GetByIdAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(restaurant);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Status.Should().Be(ResultStatus.Ok);
        await _repository.Received(1).UpdateAsync(restaurant, Arg.Any<CancellationToken>());
        restaurant.Images.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NonExistingRestaurant_ReturnsNotFound()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var command = new RemoveRestaurantImageCommand(restaurantId, imageId);

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

        var command = new RemoveRestaurantImageCommand(restaurantId, nonExistingImageId);

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
    public async Task Handle_RemoveOneOfMultipleImages_KeepsOtherImages()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var restaurant = TestDataBuilder.CreateRestaurantEntity(restaurantId, "Закусочная \"Тесткейк\"");
        restaurant.AddImage("https://example.com/image1.jpg", "Первое", 0, false);
        restaurant.AddImage("https://example.com/image2.jpg", "Второе", 1, false);

        var imageToRemove = restaurant.Images.First().Id;
        var command = new RemoveRestaurantImageCommand(restaurantId, imageToRemove);

        _repository.GetByIdAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(restaurant);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        restaurant.Images.Should().HaveCount(1);
        restaurant.Images.First().AltText.Should().Be("Второе");
    }
}
