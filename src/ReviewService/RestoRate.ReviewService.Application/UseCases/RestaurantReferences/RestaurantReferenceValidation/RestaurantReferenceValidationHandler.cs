using Ardalis.Result;
using Ardalis.SharedKernel;

using MassTransit;

using Mediator;

using RestoRate.Contracts.Restaurant;
using RestoRate.Contracts.Restaurant.Requests;
using RestoRate.ReviewService.Application.Mappings;
using RestoRate.ReviewService.Application.Sagas.Messages;
using RestoRate.ReviewService.Application.UseCases.RestaurantReferences.UpsertRestaurant;
using RestoRate.ReviewService.Domain.RestaurantReferenceAggregate;

using DomainRestaurantStatus = RestoRate.SharedKernel.Enums.RestaurantStatus;

namespace RestoRate.ReviewService.Application.UseCases.RestaurantReferences.RestaurantReferenceValidation;

public sealed class RestaurantReferenceValidationHandler(
    IRepository<RestaurantReference> repository,
    IRequestClient<GetRestaurantStatusRequest> requestClient,
    ISender sender,
    IPublishEndpoint publishEndpoint)
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
            // If the caller already knows the status (e.g. RestaurantCreated/Updated event),
            // it is safe to assume the restaurant exists unless explicitly stated otherwise.
            resolvedExists = request.Exists ?? true;

            var result = await sender.Send(
                new UpsertRestaurantCommand(restaurantId, knownStatus.ToContract()),
                cancellationToken);
            
            resolvedContractStatus = result.IsOk() ? result.Value : RestaurantStatus.Unknown;
        }
        else
        {
            var restaurantReference = await repository.GetByIdAsync(restaurantId, cancellationToken);

            if (restaurantReference is not null && restaurantReference.RestaurantStatus != DomainRestaurantStatus.Unknown)
            {
                resolvedExists = true;
                resolvedContractStatus = restaurantReference.RestaurantStatus.ToContract();
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

    private async Task<GetRestaurantStatusResponse> QueryRestaurantStatus(
        Guid restaurantId,
        CancellationToken cancellationToken)
    {
        var response = await requestClient.GetResponse<GetRestaurantStatusResponse>(
            new GetRestaurantStatusRequest(restaurantId),
            cancellationToken);

        await sender.Send(
                new UpsertRestaurantCommand(restaurantId, response.Message.Status),
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
