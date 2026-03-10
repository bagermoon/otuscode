using Ardalis.Result;

using Mediator;

using Microsoft.Extensions.Logging;

using RestoRate.Abstractions.Messaging;
using RestoRate.RatingService.Application.Mappings;
using RestoRate.RatingService.Application.Services;
using RestoRate.RatingService.Domain.Interfaces;

namespace RestoRate.RatingService.Application.UseCases.Rating.Recalculate;

public sealed class RecalculateRatingHandler(
    IRatingRecalculationDebouncer debouncer,
    IStatsCalculator statsCalculator,
    IIntegrationEventBus integrationEventBus,
    ILogger<RecalculateRatingHandler> logger)
    : ICommandHandler<RecalculateRatingCommand, Result>
{
    private static readonly TimeSpan LockDuration = TimeSpan.FromSeconds(30);

    public async ValueTask<Result> Handle(
        RecalculateRatingCommand request,
        CancellationToken cancellationToken)
    {
        if (!await debouncer.TryAcquireProcessingLockAsync(
                request.RestaurantId,
                LockDuration,
                cancellationToken))
        {
            return Result.Success();
        }

        try
        {
            var dueAt = await debouncer.GetDueAtAsync(request.RestaurantId, cancellationToken);
            if (dueAt is null || dueAt > DateTimeOffset.UtcNow)
            {
                return Result.Success();
            }

            var recalculated = await statsCalculator.RecalculateLatestAsync(request.RestaurantId, cancellationToken);

            if (!await debouncer.TryCompleteAsync(request.RestaurantId, dueAt.Value, cancellationToken))
            {
                return Result.Success();
            }

            await integrationEventBus.PublishAsync(recalculated.ToEvent(), cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogRecalculationFailed(request.RestaurantId, ex);
            return Result.Error($"Failed to recalculate rating for restaurant {request.RestaurantId}.");
        }
        finally
        {
            await debouncer.ReleaseProcessingLockAsync(request.RestaurantId, cancellationToken);
        }
    }
}