using Ardalis.SharedKernel;
using Ardalis.Specification;
using Ardalis.Result;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using NSubstitute;

using RestoRate.Contracts.Review.Dtos;
using RestoRate.ReviewService.Application.UseCases.Reviews.GetAll;
using RestoRate.ReviewService.Domain.Interfaces;
using RestoRate.SharedKernel.Filters;

namespace RestoRate.ReviewService.UnitTests.UseCases.GetAll;

public class GetAllReviewsHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_ReturnsPagedResult()
    {
        // Arrange
        var reviews = new List<Review>
        {
            Review.Create(Guid.NewGuid(), Guid.NewGuid(), 4.5m, null, "ok"),
            Review.Create(Guid.NewGuid(), Guid.NewGuid(), 3.0m, null, "meh"),
            Review.Create(Guid.NewGuid(), Guid.NewGuid(), 5.0m, null, "great")
        };

        var readRepository = Substitute.For<IReviewRepository>();
        var logger = Substitute.For<ILogger<GetAllReviewsHandler>>();

        var query = new GetAllReviewsQuery(1, 10);

        readRepository
            .ListAsync(Arg.Any<ISpecification<Review>>(), Arg.Any<BaseFilter>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<List<Review>>(new PagedInfo(1, 10, 1, 3), reviews));

        var handler = new GetAllReviewsHandler(readRepository, logger);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Items.Should().HaveCount(3);
        result.Value.TotalCount.Should().Be(3);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.Items.Should().AllBeOfType<ReviewDto>();
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectPageMetadata()
    {
        // Arrange
        var reviews = Enumerable.Range(0, 12)
            .Select(i => Review.Create(Guid.NewGuid(), Guid.NewGuid(), 4.0m, null, $"comment {i}"))
            .ToList();

        var pageItems = reviews
            .Skip(10)
            .Take(10)
            .ToList();

        var readRepository = Substitute.For<IReviewRepository>();
        var logger = Substitute.For<ILogger<GetAllReviewsHandler>>();

        var query = new GetAllReviewsQuery(2, 10);

        readRepository
            .ListAsync(Arg.Any<ISpecification<Review>>(), Arg.Any<BaseFilter>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<List<Review>>(new PagedInfo(2, 10, 2, 12), pageItems));

        var handler = new GetAllReviewsHandler(readRepository, logger);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PageNumber.Should().Be(2);
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalPages.Should().Be(2);
        result.Value.HasPreviousPage.Should().BeTrue();
        result.Value.HasNextPage.Should().BeFalse();
    }
}
