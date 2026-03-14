using Ardalis.Result;
using Ardalis.SharedKernel;

using MassTransit;

using Mediator;

using Microsoft.Extensions.Options;

using RestoRate.Contracts.Restaurant;
using RestoRate.Contracts.Restaurant.Requests;
using RestoRate.ReviewService.Application.Mappings;
using RestoRate.ReviewService.Application.Configurations;
using RestoRate.Contracts.Review.Events;
using RestoRate.ReviewService.Application.UseCases.RestaurantReferences.UpsertRestaurant;
using RestoRate.ReviewService.Domain.RestaurantReferenceAggregate;

using DomainRestaurantStatus = RestoRate.SharedKernel.Enums.RestaurantStatus;

namespace RestoRate.ReviewService.Application.UseCases.RestaurantReferences.RestaurantReferenceValidation;

public sealed class RestaurantReferenceValidationHandler(
    IRepository<RestaurantReference> repository,
    IRequestClient<GetRestaurantStatusRequest> requestClient,
    ISender sender,
    IPublishEndpoint publishEndpoint,
    IOptions<RestaurantProjectionOptions> projectionOptions)
    : ICommandHandler<RestaurantReferenceValidationCommand, Result<bool>>
{
    public async ValueTask<Result<bool>> Handle(
        RestaurantReferenceValidationCommand request,
        CancellationToken cancellationToken)
    {
        var restaurantId = request.RestaurantId;

        RestaurantStatus resolvedContractStatus;
        bool resolvedExists;
        var knownStatus = request.KnownStatus?.ToDomain();


        if (knownStatus is not null)
        {
            resolvedExists = request.Exists ?? true;

            await sender.Send(
                new UpsertRestaurantCommand(restaurantId, knownStatus.ToContract(), DateTime.UtcNow),
                cancellationToken);

            resolvedContractStatus = knownStatus.ToContract();
        }
        else
        {
            var restaurantReference = await repository.GetByIdAsync(restaurantId, cancellationToken);

            if (IsFresh(restaurantReference))
            {
                resolvedExists = true;
                resolvedContractStatus = restaurantReference!.RestaurantStatus.ToContract();
            }
            else
            {
                var response = await QueryRestaurantStatus(restaurantId, cancellationToken);
                resolvedContractStatus = response.Status;
                resolvedExists = response.Exists;
            }
        }

        return DoValidateRestaurantReference(resolvedContractStatus, resolvedExists)
            ? await NotifyRestaurantReferenceValidationOk(restaurantId, cancellationToken)
            : await NotifyRestaurantReferenceValidationFailed(restaurantId, cancellationToken);
    }

    private static bool DoValidateRestaurantReference(
        RestaurantStatus resolvedContractStatus,
        bool resolvedExists)
        => resolvedContractStatus.ToDomain().IsVisiblePublicly() && resolvedExists;

    private bool IsFresh(RestaurantReference? restaurantReference)
    {
        if (restaurantReference is null || restaurantReference.RestaurantStatus == DomainRestaurantStatus.Unknown)
        {
            return false;
        }

        if (!restaurantReference.LastSynchronizedAt.HasValue)
        {
            return false;
        }

        return DateTime.UtcNow - restaurantReference.LastSynchronizedAt.Value <= projectionOptions.Value.FreshnessTtl;
    }

    private async Task<GetRestaurantStatusResponse> QueryRestaurantStatus(
        Guid restaurantId,
        CancellationToken cancellationToken)
    {
        var response = await requestClient.GetResponse<GetRestaurantStatusResponse>(
            new GetRestaurantStatusRequest(restaurantId),
            cancellationToken);

        await sender.Send(
            new UpsertRestaurantCommand(restaurantId, response.Message.Status, DateTime.UtcNow),
            cancellationToken);

        return response.Message;
    }

    private async ValueTask<Result<bool>> NotifyRestaurantReferenceValidationOk(
        Guid restaurantId,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
                new RestaurantReferenceValidationStatus(restaurantId, true),
                publishContext => publishContext.CorrelationId = restaurantId,
                cancellationToken);

        return Result<bool>.Success(true);
    }

    private async ValueTask<Result<bool>> NotifyRestaurantReferenceValidationFailed(
        Guid restaurantId,
        CancellationToken cancellationToken)
    {
        await publishEndpoint.Publish(
                new RestaurantReferenceValidationStatus(restaurantId, false),
                publishContext => publishContext.CorrelationId = restaurantId,
                cancellationToken);

        return Result<bool>.Success(false);
    }
}
