using Ardalis.Result;
using Ardalis.SharedKernel;

using AutoFixture;
using AutoFixture.AutoNSubstitute;

using FluentAssertions;

using MassTransit;

using Mediator;

using Microsoft.Extensions.Options;

using NSubstitute;

using RestoRate.Contracts.Restaurant;
using RestoRate.Contracts.Restaurant.Requests;
using RestoRate.ReviewService.Application.Configurations;
using RestoRate.ReviewService.Application.UseCases.RestaurantReferences.RestaurantReferenceValidation;
using RestoRate.ReviewService.Application.UseCases.RestaurantReferences.UpsertRestaurant;
using RestoRate.ReviewService.Domain.RestaurantReferenceAggregate;

namespace RestoRate.ReviewService.UnitTests.UseCases.RestaurantReferences;

public sealed class RestaurantReferenceValidationHandlerTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());

    [Fact]
    public async Task Handle_WhenProjectionIsFresh_UsesLocalProjectionWithoutRemoteRefresh()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var restaurantId = Guid.NewGuid();
        var repository = _fixture.Freeze<IRepository<RestaurantReference>>();
        var requestClient = _fixture.Freeze<IRequestClient<GetRestaurantStatusRequest>>();
        var sender = _fixture.Freeze<ISender>();
        var publishEndpoint = _fixture.Freeze<IPublishEndpoint>();

        repository.GetByIdAsync(restaurantId, Arg.Any<CancellationToken>()).Returns(
            RestaurantReference.Create(
                restaurantId,
                SharedKernel.Enums.RestaurantStatus.Published,
                DateTime.UtcNow));

        var handler = CreateHandler(repository, requestClient, sender, publishEndpoint);

        var result = await handler.Handle(
            new RestaurantReferenceValidationCommand(restaurantId),
            cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        RequestClientCalls(requestClient).Should().BeEmpty();
        SenderCommands(sender).Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenProjectionIsStale_RefreshesFromRestaurantService()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var restaurantId = Guid.NewGuid();
        var repository = _fixture.Freeze<IRepository<RestaurantReference>>();
        var requestClient = _fixture.Freeze<IRequestClient<GetRestaurantStatusRequest>>();
        var sender = _fixture.Freeze<ISender>();
        var publishEndpoint = _fixture.Freeze<IPublishEndpoint>();

        repository.GetByIdAsync(restaurantId, Arg.Any<CancellationToken>()).Returns(
            RestaurantReference.Create(
                restaurantId,
                SharedKernel.Enums.RestaurantStatus.Published,
                DateTime.UtcNow.AddMinutes(-10)));

        var response = Substitute.For<Response<GetRestaurantStatusResponse>>();
        response.Message.Returns(new GetRestaurantStatusResponse(
            restaurantId,
            Exists: true,
            Status: RestaurantStatus.OnModeration));
        requestClient.GetResponse<GetRestaurantStatusResponse>(
                Arg.Any<GetRestaurantStatusRequest>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<RequestTimeout>())
            .Returns(Task.FromResult(response));

#pragma warning disable CA2012
        sender.Send(Arg.Any<UpsertRestaurantCommand>(), Arg.Any<CancellationToken>())
            .Returns(_ => SuccessfulUpsert(RestaurantStatus.OnModeration));
#pragma warning restore CA2012

        var handler = CreateHandler(repository, requestClient, sender, publishEndpoint);

        var result = await handler.Handle(
            new RestaurantReferenceValidationCommand(restaurantId),
            cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
        RequestClientCalls(requestClient).Should().HaveCount(1);
        SenderCommands(sender).Should().ContainSingle(command =>
            command.RestaurantId == restaurantId &&
            command.Status == RestaurantStatus.OnModeration &&
            command.SynchronizedAtUtc.HasValue);
    }

    [Fact]
    public async Task Handle_WhenKnownStatusProvided_RefreshesProjectionWithoutRemoteQuery()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var restaurantId = Guid.NewGuid();
        var repository = _fixture.Freeze<IRepository<RestaurantReference>>();
        var requestClient = _fixture.Freeze<IRequestClient<GetRestaurantStatusRequest>>();
        var sender = _fixture.Freeze<ISender>();
        var publishEndpoint = _fixture.Freeze<IPublishEndpoint>();

#pragma warning disable CA2012
        sender.Send(Arg.Any<UpsertRestaurantCommand>(), Arg.Any<CancellationToken>())
            .Returns(_ => SuccessfulUpsert(RestaurantStatus.Published));
#pragma warning restore CA2012

        var handler = CreateHandler(repository, requestClient, sender, publishEndpoint);

        var result = await handler.Handle(
            new RestaurantReferenceValidationCommand(
                restaurantId,
                KnownStatus: RestaurantStatus.Published,
                Exists: true),
            cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        RequestClientCalls(requestClient).Should().BeEmpty();
        SenderCommands(sender).Should().ContainSingle(command =>
            command.RestaurantId == restaurantId &&
            command.Status == RestaurantStatus.Published &&
            command.SynchronizedAtUtc.HasValue);
    }

    [Fact]
    public async Task Handle_WhenProjectionHasNoSyncTimestamp_TreatsItAsStale()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var restaurantId = Guid.NewGuid();
        var repository = _fixture.Freeze<IRepository<RestaurantReference>>();
        var requestClient = _fixture.Freeze<IRequestClient<GetRestaurantStatusRequest>>();
        var sender = _fixture.Freeze<ISender>();
        var publishEndpoint = _fixture.Freeze<IPublishEndpoint>();

        repository.GetByIdAsync(restaurantId, Arg.Any<CancellationToken>()).Returns(
            RestaurantReference.Create(
                restaurantId,
                SharedKernel.Enums.RestaurantStatus.Published));

        var response = Substitute.For<Response<GetRestaurantStatusResponse>>();
        response.Message.Returns(new GetRestaurantStatusResponse(
            restaurantId,
            Exists: true,
            Status: RestaurantStatus.Published));
        requestClient.GetResponse<GetRestaurantStatusResponse>(
                Arg.Any<GetRestaurantStatusRequest>(),
                Arg.Any<CancellationToken>(),
                Arg.Any<RequestTimeout>())
            .Returns(Task.FromResult(response));

#pragma warning disable CA2012
        sender.Send(Arg.Any<UpsertRestaurantCommand>(), Arg.Any<CancellationToken>())
            .Returns(_ => SuccessfulUpsert(RestaurantStatus.Published));
#pragma warning restore CA2012

        var handler = CreateHandler(repository, requestClient, sender, publishEndpoint);

        var result = await handler.Handle(
            new RestaurantReferenceValidationCommand(restaurantId),
            cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        RequestClientCalls(requestClient).Should().HaveCount(1);
    }

    private static RestaurantReferenceValidationHandler CreateHandler(
        IRepository<RestaurantReference> repository,
        IRequestClient<GetRestaurantStatusRequest> requestClient,
        ISender sender,
        IPublishEndpoint publishEndpoint)
        => new(
            repository,
            requestClient,
            sender,
            publishEndpoint,
            Options.Create(new RestaurantProjectionOptions
            {
                FreshnessTtl = TimeSpan.FromMinutes(5)
            }));

    private static ValueTask<Result<RestaurantStatus>> SuccessfulUpsert(RestaurantStatus status)
        => ValueTask.FromResult(Result<RestaurantStatus>.Success(status));

    private static GetRestaurantStatusRequest[] RequestClientCalls(IRequestClient<GetRestaurantStatusRequest> requestClient)
        => requestClient.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(IRequestClient<GetRestaurantStatusRequest>.GetResponse))
            .Select(call => call.GetArguments().OfType<GetRestaurantStatusRequest>().Single())
            .ToArray();

    private static UpsertRestaurantCommand[] SenderCommands(ISender sender)
        => sender.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == nameof(ISender.Send))
            .Select(call => call.GetArguments().OfType<UpsertRestaurantCommand>().Single())
            .ToArray();
}