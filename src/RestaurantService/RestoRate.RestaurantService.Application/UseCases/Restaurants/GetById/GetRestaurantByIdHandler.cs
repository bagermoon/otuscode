using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using Microsoft.Extensions.Logging;

using RestoRate.Contracts.Restaurant.DTOs;
using RestoRate.RestaurantService.Domain.RestaurantAggregate;
using RestoRate.RestaurantService.Domain.RestaurantAggregate.Specifications;
using RestoRate.RestaurantService.Application.Mappings;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.GetById;

public sealed class GetRestaurantByIdHandler(
    IReadRepository<Restaurant> readRepository,
    ILogger<GetRestaurantByIdHandler> logger)
    : IQueryHandler<GetRestaurantByIdQuery, Result<RestaurantDto>>
{
    public async ValueTask<Result<RestaurantDto>> Handle(
        GetRestaurantByIdQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogGettingById(request.RestaurantId);

        try
        {
            var spec = new GetRestaurantByIdSpec(request.RestaurantId);
            var restaurant = await readRepository.FirstOrDefaultAsync(spec, cancellationToken);

            if (restaurant == null)
            {
                logger.LogRestaurantNotFound(request.RestaurantId);
                return Result.NotFound();
            }

            var dto = restaurant.ToDto();

            logger.LogFound(request.RestaurantId);
            return Result<RestaurantDto>.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogGetError(ex);
            return Result<RestaurantDto>.Error(ex.Message);
        }
    }
}
