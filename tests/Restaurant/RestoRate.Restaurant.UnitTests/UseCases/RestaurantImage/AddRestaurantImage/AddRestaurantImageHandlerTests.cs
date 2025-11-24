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
using RestoRate.Restaurant.Application.UseCases.RestaurantImage.AddRestaurantImage;
using RestoRate.Restaurant.UnitTests.Helpers;
using RestaurantEntity = RestoRate.Restaurant.Domain.RestaurantAggregate.Restaurant;

namespace RestoRate.Restaurant.UnitTests.UseCases.RestaurantImage.AddRestaurantImage;

public sealed class AddRestaurantImageHandlerTests
{
    private readonly IRepository<RestaurantEntity> _repository;
    private readonly ILogger<AddRestaurantImageHandler> _logger;
    private readonly AddRestaurantImageHandler _handler;

    public AddRestaurantImageHandlerTests()
    {
        _repository = Substitute.For<IRepository<RestaurantEntity>>();
        _logger = Substitute.For<ILogger<AddRestaurantImageHandler>>();
        _handler = new AddRestaurantImageHandler(_repository, _logger);
    }

    [Fact]
    public async Task Handle_ValidRequest_AddsImageSuccessfully()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var restaurant = TestDataBuilder.CreateRestaurantEntity(restaurantId, "Закусочная \"Тесткейк\"");
        var command = new AddRestaurantImageCommand(
            restaurantId,
            "https://example.com/image.jpg",
            "Красивое фото",
            false);

        _repository.GetByIdAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(restaurant);

        var initialImageCount = restaurant.Images.Count;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);

        await _repository.Received(1).UpdateAsync(restaurant, Arg.Any<CancellationToken>());

        restaurant.Images.Should().HaveCount(initialImageCount + 1);

        var addedImage = restaurant.Images.FirstOrDefault(img => img.Url == "https://example.com/image.jpg");
        addedImage.Should().NotBeNull();
        addedImage!.AltText.Should().Be("Красивое фото");
        addedImage.IsPrimary.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_AddPrimaryImage_SetsPrimaryCorrectly()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var restaurant = TestDataBuilder.CreateRestaurantEntity(restaurantId, "Закусочная \"Тесткейк\"");
        var command = new AddRestaurantImageCommand(
            restaurantId,
            "https://example.com/primary.jpg",
            "Главное фото",
            true);

        _repository.GetByIdAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(restaurant);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        restaurant.Images.First().IsPrimary.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NonExistingRestaurant_ReturnsNotFound()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var command = new AddRestaurantImageCommand(
            restaurantId,
            "https://example.com/image.jpg",
            "Фото",
            false);

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
    public async Task Handle_MultipleImages_SetsCorrectDisplayOrder()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var restaurant = TestDataBuilder.CreateRestaurantEntity(restaurantId, "Закусочная \"Тесткейк\"");

        // Добавляем первое изображение
        restaurant.AddImage("https://example.com/image1.jpg", "Первое", 0, false);

        var command = new AddRestaurantImageCommand(
            restaurantId,
            "https://example.com/image2.jpg",
            "Второе",
            false);

        _repository.GetByIdAsync(restaurantId, Arg.Any<CancellationToken>())
            .Returns(restaurant);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        restaurant.Images.Should().HaveCount(2);
        restaurant.Images.Last().DisplayOrder.Should().Be(1);
    }
}
