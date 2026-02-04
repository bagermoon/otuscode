using AutoFixture;
using AutoFixture.AutoNSubstitute;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using NSubstitute;

using RestoRate.Abstractions.Identity;
using RestoRate.Contracts.Common.Dtos;
using RestoRate.Contracts.Review.Dtos;
using RestoRate.ReviewService.Application.UseCases.Reviews.Create;

namespace RestoRate.ReviewService.UnitTests.UseCases.Create;

public class CreateReviewHandlerTests
{
    private readonly IFixture _fixture;

    public CreateReviewHandlerTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public async Task Handle_ValidCommand_AddsReviewAndReturnsDto()
    {
        // Arrange
        var dto = _fixture.Build<CreateReviewDto>()
            .With(x => x.Comment, "Great food")
            .With(x => x.AverageCheck, (MoneyDto?)null)
            .Create();
        var cmd = new CreateReviewCommand(dto);

        var repo = _fixture.Freeze<Ardalis.SharedKernel.IRepository<Review>>();
        repo.AddAsync(Arg.Any<Review>(), Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(ci.ArgAt<Review>(0)));

        var logger = _fixture.Freeze<ILogger<CreateReviewHandler>>();

        var userContext = _fixture.Freeze<IUserContext>();
        userContext.IsAuthenticated.Returns(true);
        userContext.UserId.Returns(dto.UserId);

        var handler = new CreateReviewHandler(repo, userContext, logger);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var resDto = result.Value;
        resDto.Should().NotBeNull();
        resDto.RestaurantId.Should().Be(dto.RestaurantId);
        resDto.UserId.Should().Be(dto.UserId);
        resDto.Rating.Should().Be(dto.Rating);
        resDto.AverageCheck.Should().Be(dto.AverageCheck);
        resDto.Comment.Should().Be(dto.Comment);
    }

    [Fact]
    public async Task Handle_RepositoryThrows_ReturnsError()
    {
        // Arrange
        var dto = _fixture.Create<CreateReviewDto>();
        var cmd = new CreateReviewCommand(dto);

        var repo = _fixture.Freeze<Ardalis.SharedKernel.IRepository<Review>>();
        repo.AddAsync(Arg.Any<Review>(), Arg.Any<CancellationToken>())
            .Returns<Task<Review>>(_ => throw new System.InvalidOperationException("DB down"));

        var logger = _fixture.Freeze<ILogger<CreateReviewHandler>>();

        var userContext = _fixture.Freeze<IUserContext>();
        userContext.IsAuthenticated.Returns(true);
        userContext.UserId.Returns(dto.UserId);

        var handler = new CreateReviewHandler(repo, userContext, logger);

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
}
