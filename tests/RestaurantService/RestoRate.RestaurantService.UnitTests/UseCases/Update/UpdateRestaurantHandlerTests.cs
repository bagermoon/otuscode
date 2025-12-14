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
using RestoRate.RestaurantService.Application.UseCases.Update;
using RestoRate.RestaurantService.Domain.Interfaces;
using RestoRate.RestaurantService.Domain.TagAggregate;
using RestoRate.RestaurantService.UnitTests.Helpers;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;

namespace RestoRate.RestaurantService.UnitTests.UseCases.Update;
public class UpdateRestaurantHandlerTests
{
    private readonly IRestaurantService _restaurantService;
    private readonly IRepository<Tag> _tagRepository;
    private readonly ILogger<UpdateRestaurantHandler> _logger;
    private readonly UpdateRestaurantHandler _handler;
    private readonly ITestOutputHelper _output;

    public UpdateRestaurantHandlerTests(ITestOutputHelper output)
    {
        _output = output;
        _restaurantService = Substitute.For<IRestaurantService>();
        _tagRepository = Substitute.For<IRepository<Tag>>();
        _logger = Substitute.For<ILogger<UpdateRestaurantHandler>>();
        _handler = new UpdateRestaurantHandler(_restaurantService, _tagRepository, _logger);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var dto = TestDataBuilder.CreateValidUpdateRestaurantDto(restaurantId, "Ресторан \"Обновлень\"");
        var command = new UpdateRestaurantCommand(dto);

        _restaurantService
            .UpdateRestaurant(
                restaurantId,
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<PhoneNumber>(),
                Arg.Any<Email>(),
                Arg.Any<Address>(),
                Arg.Any<Location>(),
                Arg.Any<OpenHours>(),
                Arg.Any<Money>(),
                Arg.Any<IEnumerable<CuisineType>>(),
                Arg.Any<IEnumerable<Tag>>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _restaurantService
            .Received(1)
            .UpdateRestaurant(
                restaurantId,
                dto.Name,
                dto.Description,
                Arg.Any<PhoneNumber>(),
                Arg.Any<Email>(),
                Arg.Any<Address>(),
                Arg.Any<Location>(),
                Arg.Any<OpenHours>(),
                Arg.Any<Money>(),
                Arg.Any<IEnumerable<CuisineType>>(),
                Arg.Any<IEnumerable<Tag>>());
    }

    [Fact]
    public async Task Handle_NonExistingRestaurant_ReturnsNotFound()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var dto = TestDataBuilder.CreateValidUpdateRestaurantDto(restaurantId);
        var command = new UpdateRestaurantCommand(dto);

        _restaurantService
            .UpdateRestaurant(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<PhoneNumber>(),
                Arg.Any<Email>(),
                Arg.Any<Address>(),
                Arg.Any<Location>(),
                Arg.Any<OpenHours>(),
                Arg.Any<Money>(),
                Arg.Any<IEnumerable<CuisineType>>(),
                Arg.Any<IEnumerable<Tag>>())
            .Returns(Task.FromResult(Result.NotFound()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_UpdateWithNewCuisineTypes_ReturnsSuccess()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var dto = TestDataBuilder.CreateValidUpdateRestaurantDto(
            restaurantId,
            cuisineTypes: new[] { CuisineType.Chinese.Name, CuisineType.Thai.Name });
        var command = new UpdateRestaurantCommand(dto);

        _restaurantService
            .UpdateRestaurant(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<PhoneNumber>(),
                Arg.Any<Email>(),
                Arg.Any<Address>(),
                Arg.Any<Location>(),
                Arg.Any<OpenHours>(),
                Arg.Any<Money>(),
                Arg.Is<IEnumerable<CuisineType>>(ct => ct.Count() == 2),
                Arg.Any<IEnumerable<Tag>>())
            .Returns(Task.FromResult(Result.Success()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _restaurantService
            .Received(1)
            .UpdateRestaurant(
                restaurantId,
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<PhoneNumber>(),
                Arg.Any<Email>(),
                Arg.Any<Address>(),
                Arg.Any<Location>(),
                Arg.Any<OpenHours>(),
                Arg.Any<Money>(),
                Arg.Is<IEnumerable<CuisineType>>(ct => ct.Count() == 2),
                Arg.Any<IEnumerable<Tag>>());
    }

    [Fact]
    public async Task Handle_ServiceReturnsError_ReturnsError()
    {
        // Arrange
        var restaurantId = Guid.NewGuid();
        var dto = TestDataBuilder.CreateValidUpdateRestaurantDto(restaurantId);
        var command = new UpdateRestaurantCommand(dto);
        var errorMessage = "Ошибка обновления";

        _restaurantService
            .UpdateRestaurant(
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<PhoneNumber>(),
                Arg.Any<Email>(),
                Arg.Any<Address>(),
                Arg.Any<Location>(),
                Arg.Any<OpenHours>(),
                Arg.Any<Money>(),
                Arg.Any<IEnumerable<CuisineType>>(),
                Arg.Any<IEnumerable<Tag>>())
            .Returns(Task.FromResult(Result.Error(errorMessage)));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(errorMessage);
    }
}
