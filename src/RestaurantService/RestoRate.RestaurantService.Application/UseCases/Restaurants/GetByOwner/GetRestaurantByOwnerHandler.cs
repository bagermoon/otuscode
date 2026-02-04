using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using Microsoft.Extensions.Logging;

using RestoRate.Contracts.Restaurant.DTOs;
using RestoRate.RestaurantService.Application.Mappings;
using RestoRate.RestaurantService.Domain.RestaurantAggregate;
using RestoRate.RestaurantService.Domain.RestaurantAggregate.Specifications;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.GetByOwner;

public sealed class GetRestaurantByOwnerHandler(IReadRepository<Restaurant> readRepository)
    : IQueryHandler<GetRestaurantByOwnerQuery, Result<List<RestaurantDto>>>
{
    public async ValueTask<Result<List<RestaurantDto>>> Handle(
        GetRestaurantByOwnerQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var spec = new GetRestaurantsByOwnerSpec(request.OwnerId);
            var restaurants = await readRepository.ListAsync(spec, cancellationToken);
            var dtos = restaurants.Select(r => r.ToDto()).ToList();

            return Result<List<RestaurantDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<RestaurantDto>>.Error(ex.Message);
        }
    }
}
