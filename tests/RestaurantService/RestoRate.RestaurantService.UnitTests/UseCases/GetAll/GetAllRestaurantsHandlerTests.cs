using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ardalis.SharedKernel;
using Ardalis.Specification;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RestoRate.RestaurantService.Application.UseCases.GetAll;
using RestoRate.RestaurantService.UnitTests.Helpers;
using RestoRate.SharedKernel.Enums;
using RestaurantEntity = RestoRate.RestaurantService.Domain.RestaurantAggregate.Restaurant;

namespace RestoRate.RestaurantService.UnitTests.UseCases.GetAll;
public class GetAllRestaurantsHandlerTests
{
    private readonly IReadRepository<RestaurantEntity> _readRepository;
    private readonly ILogger<GetAllRestaurantsHandler> _logger;
    private readonly GetAllRestaurantsHandler _handler;
    private readonly ITestOutputHelper _output;

    public GetAllRestaurantsHandlerTests(ITestOutputHelper output)
    {
        _output = output;
        _readRepository = Substitute.For<IReadRepository<RestaurantEntity>>();
        _logger = Substitute.For<ILogger<GetAllRestaurantsHandler>>();
        _handler = new GetAllRestaurantsHandler(_readRepository, _logger);
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsPagedResult()
    {
        // Arrange
        var restaurants = new List<RestaurantEntity>
        {
            TestDataBuilder.CreateRestaurantEntity(Guid.NewGuid(), "Мимино"),
            TestDataBuilder.CreateRestaurantEntity(Guid.NewGuid(), "Барракуда"),
            TestDataBuilder.CreateRestaurantEntity(Guid.NewGuid(), "АйДаБаран")
        };

        var query = new GetAllRestaurantsQuery(1, 10);

        _readRepository
            .ListAsync(
                Arg.Any<ISpecification<RestaurantEntity>>(),
                Arg.Any<CancellationToken>())
            .Returns(restaurants);

        _readRepository
            .CountAsync(
                Arg.Any<ISpecification<RestaurantEntity>>(),
                Arg.Any<CancellationToken>())
            .Returns(3);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
        result.Value.TotalCount.Should().Be(3);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_EmptyResult_ReturnsEmptyList()
    {
        // Arrange
        var query = new GetAllRestaurantsQuery(1, 10);

        _readRepository
            .ListAsync(
                Arg.Any<ISpecification<RestaurantEntity>>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<RestaurantEntity>());

        _readRepository
            .CountAsync(
                Arg.Any<ISpecification<RestaurantEntity>>(),
                Arg.Any<CancellationToken>())
            .Returns(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_FiltersResults()
    {
        // Arrange
        var restaurants = new List<RestaurantEntity>
        {
            TestDataBuilder.CreateRestaurantEntity(Guid.NewGuid(), "Додо пицца")
        };

        var query = new GetAllRestaurantsQuery(1, 10, "Пицца");

        _readRepository
            .ListAsync(
                Arg.Any<ISpecification<RestaurantEntity>>(),
                Arg.Any<CancellationToken>())
            .Returns(restaurants);

        _readRepository
            .CountAsync(
                Arg.Any<ISpecification<RestaurantEntity>>(),
                Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().Name.Should().ContainEquivalentOf("Пицца");
    }

    [Fact]
    public async Task Handle_WithCuisineTypeFilter_ReturnsFilteredResults()
    {
        // Arrange
        var restaurants = new List<RestaurantEntity>
        {
            TestDataBuilder.CreateRestaurantEntity(Guid.NewGuid(), "Вкусы Италии")
        };

        var query = new GetAllRestaurantsQuery(1, 10, CuisineType: CuisineType.Italian.Name);

        _readRepository
            .ListAsync(
                Arg.Any<ISpecification<RestaurantEntity>>(),
                Arg.Any<CancellationToken>())
            .Returns(restaurants);

        _readRepository
            .CountAsync(
                Arg.Any<ISpecification<RestaurantEntity>>(),
                Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectPage()
    {
        // Arrange
        var restaurants = new List<RestaurantEntity>
        {
            TestDataBuilder.CreateRestaurantEntity(Guid.NewGuid(), "Мама на кухне"),
            TestDataBuilder.CreateRestaurantEntity(Guid.NewGuid(), "Суша и море")
        };

        var query = new GetAllRestaurantsQuery(2, 10); // Вторая страница

        _readRepository
            .ListAsync(
                Arg.Any<ISpecification<RestaurantEntity>>(),
                Arg.Any<CancellationToken>())
            .Returns(restaurants);

        _readRepository
            .CountAsync(
                Arg.Any<ISpecification<RestaurantEntity>>(),
                Arg.Any<CancellationToken>())
            .Returns(12);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PageNumber.Should().Be(2);
        result.Value.TotalPages.Should().Be(2);
        result.Value.HasPreviousPage.Should().BeTrue();
        result.Value.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ReturnsError()
    {
        // Arrange
        var query = new GetAllRestaurantsQuery(1, 10);
        var errorMessage = "Ошибка подключения к БД";

        _readRepository
            .ListAsync(
                Arg.Any<ISpecification<RestaurantEntity>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<RestaurantEntity>>(new Exception(errorMessage)));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(errorMessage);
    }
}
