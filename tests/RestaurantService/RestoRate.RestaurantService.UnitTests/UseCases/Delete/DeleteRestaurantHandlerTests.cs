using Ardalis.Result;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using NSubstitute;

using RestoRate.RestaurantService.Application.UseCases.Restaurants.Delete;
using RestoRate.RestaurantService.Domain.Interfaces;

namespace RestoRate.RestaurantService.UnitTests.UseCases.Delete;
public class DeleteRestaurantHandlerTests
{
    private readonly IRestaurantService _restaurantService;
    private readonly ILogger<DeleteRestaurantHandler> _logger;
    private readonly DeleteRestaurantHandler _handler;
    private readonly ITestOutputHelper _output;

    public DeleteRestaurantHandlerTests(ITestOutputHelper output)
    {
        _output = output;
        _restaurantService = Substitute.For<IRestaurantService>();
        _logger = Substitute.For<ILogger<DeleteRestaurantHandler>>();
        _handler = new DeleteRestaurantHandler(_restaurantService, _logger);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var command = new DeleteRestaurantCommand(restaurantId);

        _restaurantService
            .DeleteRestaurantAsync(restaurantId)
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _restaurantService
            .Received(1)
            .DeleteRestaurantAsync(restaurantId);
    }

    [Fact]
    public async Task Handle_NonExistingRestaurant_ReturnsNotFound()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var command = new DeleteRestaurantCommand(restaurantId);

        _restaurantService
            .DeleteRestaurantAsync(restaurantId)
            .Returns(Task.FromResult(Result.NotFound()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_ServiceThrowsException_ReturnsError()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var command = new DeleteRestaurantCommand(restaurantId);
        var errorMessage = "Ошибка базы данных";

        _restaurantService
            .DeleteRestaurantAsync(restaurantId)
            .Returns(Task.FromException<Result>(new Exception(errorMessage)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(errorMessage);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("123e4567-e89b-12d3-a456-426614174000")]
    public async Task Handle_DifferentGuids_CallsServiceWithCorrectId(string guidString)
    {
        // Arrange
        var restaurantId = Guid.Parse(guidString);
        var command = new DeleteRestaurantCommand(restaurantId);

        _restaurantService
            .DeleteRestaurantAsync(restaurantId)
            .Returns(Task.FromResult(Result.Success()));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _restaurantService
            .Received(1)
            .DeleteRestaurantAsync(restaurantId);
    }
}
