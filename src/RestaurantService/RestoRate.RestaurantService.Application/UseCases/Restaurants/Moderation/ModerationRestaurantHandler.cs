using System;
using System.Threading;
using System.Threading.Tasks;

using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using Microsoft.Extensions.Logging;

using RestoRate.Contracts.Restaurant;
using RestoRate.RestaurantService.Application.Mappings;
using RestoRate.RestaurantService.Domain.RestaurantAggregate;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.Moderation;

/// <summary>
/// Handles moderation operations for restaurants (status changes such as Approve/Reject/Block).
/// This class contains lightweight validation and coordinates repository calls.
/// </summary>
public sealed class ModerationRestaurantHandler(
    IRepository<Restaurant> repository,
    ILogger<ModerationRestaurantHandler> logger
) : ICommandHandler<ModerationRestaurantCommand, Result>
{
    private readonly IRepository<Restaurant> _repository = repository;
    private readonly ILogger<ModerationRestaurantHandler> _logger = logger;

    /// <summary>
    /// Update restaurant moderation status.
    /// </summary>
    public async ValueTask<Result> Handle(
        ModerationRestaurantCommand request,
        CancellationToken cancellationToken = default
    )
    {
        var restaurant = await _repository.GetByIdAsync(request.RestaurantId, cancellationToken);
        if (restaurant is null)
        {
            _logger.LogRestaurantNotFound(request.RestaurantId);
            return Result.NotFound();
        }

        var result = restaurant.UpdateStatus(request.Status.ToDomain());
        if (result.IsError())
        {
            _logger.LogFailedToUpdateStatus(request.RestaurantId, result.Errors);
            return result;
        }

        try
        {
            await _repository.UpdateAsync(restaurant, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogFailedToUpdateStatus(request.RestaurantId, ex.Message);
            return Result.CriticalError("Failed set restaurant status. {message}", ex.Message);
        }

        return Result.NoContent();
    }
}