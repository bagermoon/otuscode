using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RestoRate.Abstractions.Messaging;
using RestoRate.Contracts.Restaurant.Events;
using RestoRate.Restaurant.Application.UseCases.Create;
using RestoRate.Restaurant.Domain.Interfaces;
using RestoRate.Restaurant.UnitTests.Helpers;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;

namespace RestoRate.Restaurant.UnitTests.UseCases.Create
{
    public class CreateRestaurantHandlerTests
    {
        private readonly IRestaurantService _restaurantService;
        private readonly IIntegrationEventBus _integrationEventBus;
        private readonly ILogger<CreateRestaurantHandler> _logger;
        private readonly CreateRestaurantHandler _handler;
        private readonly ITestOutputHelper _output;

        public CreateRestaurantHandlerTests(ITestOutputHelper output)
        {
            _output = output;
            _restaurantService = Substitute.For<IRestaurantService>();
            _integrationEventBus = Substitute.For<IIntegrationEventBus>();
            _logger = Substitute.For<ILogger<CreateRestaurantHandler>>();

            _handler = new CreateRestaurantHandler(
                _restaurantService,
                _integrationEventBus,
                _logger);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsSuccess()
        {
            // Arrange
            var restaurantId = Guid.NewGuid();
            var dto = TestDataBuilder.CreateValidRestaurantDto();
            var command = new CreateRestaurantCommand(dto);

            _restaurantService
                .CreateRestaurant(
                    Arg.Any<string>(),
                    Arg.Any<string?>(),
                    Arg.Any<PhoneNumber>(),
                    Arg.Any<Email>(),
                    Arg.Any<Address>(),
                    Arg.Any<Location>(),
                    Arg.Any<OpenHours>(),
                    Arg.Any<Money>(),
                    Arg.Any<IEnumerable<CuisineType>>(),
                    Arg.Any<IEnumerable<Tag>>(),
                    Arg.Any<IEnumerable<(string, string?, bool)>?>())
                .Returns(Task.FromResult(Result<Guid>.Success(restaurantId)));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue($"Ожидал успеха, но получил: {string.Join(", ", result.Errors)}");
            result.Value.Should().NotBeNull();
            result.Value.RestaurantId.Should().Be(restaurantId);
            result.Value.Name.Should().Be(dto.Name);

            await _integrationEventBus
                .Received(1)
                .PublishAsync( // проверка ивента
                    Arg.Any<RestaurantCreatedEvent>(),
                    Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WithMultipleCuisineTypes_ReturnsSuccess()
        {
            // Arrange
            var restaurantId = Guid.NewGuid();

            var dto = TestDataBuilder.CreateValidRestaurantDto(
                cuisineTypes: new[]
                {
                    CuisineType.Italian.Name,
                    CuisineType.French.Name,
                    CuisineType.Japanese.Name
                });

            var command = new CreateRestaurantCommand(dto);

            _restaurantService
                .CreateRestaurant(
                    Arg.Any<string>(),
                    Arg.Any<string?>(),
                    Arg.Any<PhoneNumber>(),
                    Arg.Any<Email>(),
                    Arg.Any<Address>(),
                    Arg.Any<Location>(),
                    Arg.Any<OpenHours>(),
                    Arg.Any<Money>(),
                    Arg.Any<IEnumerable<CuisineType>>(),
                    Arg.Any<IEnumerable<Tag>>(),
                    Arg.Any<IEnumerable<(string, string?, bool)>?>())
                .Returns(Task.FromResult(Result<Guid>.Success(restaurantId)));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.CuisineTypes.Should().HaveCount(3);
            result.Value.CuisineTypes.Should().Contain(CuisineType.Italian.Name);
        }

        [Fact]
        public async Task Handle_WithImages_ReturnsSuccess()
        {
            // Arrange
            var restaurantId = Guid.NewGuid();

            var images = new[]
            {
                TestDataBuilder.CreateValidImageDto(
                    url: "https://example.com/image1.jpg",
                    isPrimary: true),
                TestDataBuilder.CreateValidImageDto(
                    url: "https://example.com/image2.jpg",
                    isPrimary: false)
            };

            var dto = TestDataBuilder.CreateValidRestaurantDto(images: images);
            var command = new CreateRestaurantCommand(dto);

            _restaurantService
                .CreateRestaurant(
                    Arg.Any<string>(),
                    Arg.Any<string?>(),
                    Arg.Any<PhoneNumber>(),
                    Arg.Any<Email>(),
                    Arg.Any<Address>(),
                    Arg.Any<Location>(),
                    Arg.Any<OpenHours>(),
                    Arg.Any<Money>(),
                    Arg.Any<IEnumerable<CuisineType>>(),
                    Arg.Any<IEnumerable<Tag>>(),
                    Arg.Any<IEnumerable<(string, string?, bool)>?>())
                .Returns(Task.FromResult(Result<Guid>.Success(restaurantId)));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            await _restaurantService
                .Received(1)
                .CreateRestaurant(
                    Arg.Any<string>(),
                    Arg.Any<string?>(),
                    Arg.Any<PhoneNumber>(),
                    Arg.Any<Email>(),
                    Arg.Any<Address>(),
                    Arg.Any<Location>(),
                    Arg.Any<OpenHours>(),
                    Arg.Any<Money>(),
                    Arg.Any<IEnumerable<CuisineType>>(),
                    Arg.Any<IEnumerable<Tag>>(),
                    Arg.Is<IEnumerable<(string, string?, bool)>?>(imgs => imgs != null && imgs.Count() == 2));
        }

        [Theory]
        [InlineData("Bella Italia", "Italian")]
        [InlineData("Sakura", "Japanese")]
        [InlineData("Le Petit Bistro", "French")]
        public async Task Handle_DifferentCuisineTypes_ReturnsSuccess(string name, string cuisineType)
        {
            // Arrange
            var restaurantId = Guid.NewGuid();
            var dto = TestDataBuilder.CreateValidRestaurantDto(
                name: name,
                cuisineTypes: new[] { cuisineType });

            var command = new CreateRestaurantCommand(dto);

            _restaurantService
                .CreateRestaurant(
                    Arg.Any<string>(),
                    Arg.Any<string?>(),
                    Arg.Any<PhoneNumber>(),
                    Arg.Any<Email>(),
                    Arg.Any<Address>(),
                    Arg.Any<Location>(),
                    Arg.Any<OpenHours>(),
                    Arg.Any<Money>(),
                    Arg.Any<IEnumerable<CuisineType>>(),
                    Arg.Any<IEnumerable<Tag>>(),
                    Arg.Any<IEnumerable<(string, string?, bool)>?>())
                .Returns(Task.FromResult(Result<Guid>.Success(restaurantId)));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Name.Should().Be(name);
            result.Value.CuisineTypes.Should().Contain(cuisineType);
        }

        [Fact]
        public async Task Handle_ServiceReturnsError_ReturnsError()
        {
            // Arrange
            var dto = TestDataBuilder.CreateValidRestaurantDto();
            var command = new CreateRestaurantCommand(dto);
            var errorMessage = "Не удалось подключиться к базе данных";

            _restaurantService
                .CreateRestaurant(
                    Arg.Any<string>(),
                    Arg.Any<string?>(),
                    Arg.Any<PhoneNumber>(),
                    Arg.Any<Email>(),
                    Arg.Any<Address>(),
                    Arg.Any<Location>(),
                    Arg.Any<OpenHours>(),
                    Arg.Any<Money>(),
                    Arg.Any<IEnumerable<CuisineType>>(),
                    Arg.Any<IEnumerable<Tag>>(),
                    Arg.Any<IEnumerable<(string, string?, bool)>?>())
                .Returns(Task.FromResult(Result<Guid>.Error(errorMessage)));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(errorMessage);

            await _integrationEventBus
                .DidNotReceive()
                .PublishAsync( // проверка ивента
                    Arg.Any<RestaurantCreatedEvent>(),
                    Arg.Any<CancellationToken>());
        }
    }
}
