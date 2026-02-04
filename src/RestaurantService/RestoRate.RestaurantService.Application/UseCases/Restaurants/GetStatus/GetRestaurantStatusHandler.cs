using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using RestoRate.RestaurantService.Domain.RestaurantAggregate;
using RestoRate.SharedKernel.Enums;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.GetStatus;

public sealed class GetRestaurantStatusHandler(
    IReadRepository<Restaurant> readRepository)
    : IQueryHandler<GetRestaurantStatusQuery, Result<GetRestaurantStatusResult>>
{
    public async ValueTask<Result<GetRestaurantStatusResult>> Handle(
        GetRestaurantStatusQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var restaurant = await readRepository.GetByIdAsync(request.RestaurantId, cancellationToken);

            return Result.Success(new GetRestaurantStatusResult(
                RestaurantId: request.RestaurantId,
                Exists: restaurant is not null,
                Status: restaurant is not null ? restaurant.RestaurantStatus : RestaurantStatus.Unknown));
        }
        catch (Exception ex)
        {
            return Result.Error(ex.Message);
        }
    }
}
