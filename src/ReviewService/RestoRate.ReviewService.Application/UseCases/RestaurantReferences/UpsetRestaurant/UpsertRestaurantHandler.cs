using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using Microsoft.Extensions.Logging;

using RestoRate.Contracts.Restaurant;
using RestoRate.ReviewService.Application.Mappings;
using RestoRate.ReviewService.Domain.RestaurantReferenceAggregate;

namespace RestoRate.ReviewService.Application.UseCases.RestaurantReferences.UpsertRestaurant;

public sealed class UpsertRestaurantHandler(
    IRepository<RestaurantReference> repository,
    ILogger<UpsertRestaurantHandler> logger)
    : ICommandHandler<UpsertRestaurantCommand, Result<RestaurantStatus>>
{
    public async ValueTask<Result<RestaurantStatus>> Handle(
        UpsertRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        var restaurantReference = await repository.GetByIdAsync(request.RestaurantId, cancellationToken);

        // If already present, update only when needed; otherwise return what's stored.
        if (restaurantReference is not null)
        {
            var requestedStatus = request.Status.ToDomain();
            if (restaurantReference.RestaurantStatus == requestedStatus)
            {
                return Result<RestaurantStatus>.Success(restaurantReference.RestaurantStatus.ToContract());
            }

            restaurantReference.UpdateStatus(requestedStatus);
            await repository.UpdateAsync(restaurantReference, cancellationToken);
            return Result<RestaurantStatus>.Success(restaurantReference.RestaurantStatus.ToContract());
        }

        var newReference = RestaurantReference.Create(
            request.RestaurantId,
            request.Status.ToDomain());

        try
        {
            await repository.AddAsync(newReference, cancellationToken);
            return Result<RestaurantStatus>.Success(newReference.RestaurantStatus.ToContract());
        }
        catch (Exception ex) when (LooksLikeAlreadyExists(ex))
        {
            logger.LogRestaurantReferenceAlreadyExists(request.RestaurantId);

            // A concurrent insert won the race; re-read and return the stored value.
            var existing = await repository.GetByIdAsync(request.RestaurantId, cancellationToken);
            if (existing is not null)
            {
                return Result<RestaurantStatus>.Success(existing.RestaurantStatus.ToContract());
            }

            return Result<RestaurantStatus>.Error(
                $"RestaurantReference insert raced for {request.RestaurantId}, but could not be reloaded.");
        }
    }

    private static bool LooksLikeAlreadyExists(Exception ex)
    {
        // Keep Application layer decoupled from EF/Mongo exception types; detect by common duplicate-key messages.
        for (var current = ex; current is not null; current = current.InnerException)
        {
            var message = current.Message;
            if (!string.IsNullOrWhiteSpace(message) &&
                (message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
                 message.Contains("E11000", StringComparison.OrdinalIgnoreCase) ||
                 message.Contains("already exists", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
        }

        return false;
    }
}
