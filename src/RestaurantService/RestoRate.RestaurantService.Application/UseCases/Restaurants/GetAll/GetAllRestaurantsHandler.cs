using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using Microsoft.Extensions.Logging;
using RestoRate.Contracts.Restaurant.DTOs;
using RestoRate.RestaurantService.Domain.RestaurantAggregate.Specifications;

using RestaurantEntity = RestoRate.RestaurantService.Domain.RestaurantAggregate.Restaurant;
using RestoRate.RestaurantService.Application.Mappings;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.GetAll;

public sealed class GetAllRestaurantsHandler(
    IReadRepository<RestaurantEntity> readRepository,
    ILogger<GetAllRestaurantsHandler> logger)
    : IQueryHandler<GetAllRestaurantsQuery, Result<Contracts.Common.PagedResult<RestaurantDto>>>
{
    public async ValueTask<Result<Contracts.Common.PagedResult<RestaurantDto>>> Handle(
        GetAllRestaurantsQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogGettingList(request.PageNumber, request.PageSize, request.SearchTerm);

        try
        {
            // Создаем Specification с фильтрами
            var spec = new GetAllRestaurantsSpec(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                request.CuisineType,
                request.Tag);

            var restaurants = await readRepository.ListAsync(spec, cancellationToken);
            var totalCount = await readRepository.CountAsync(spec, cancellationToken);

            var dtos = restaurants.Select(r => r.ToDto()).ToList();

            var result = new Contracts.Common.PagedResult<RestaurantDto>(
                dtos,
                totalCount,
                request.PageNumber,
                request.PageSize
            );

            logger.LogFoundCount(dtos.Count, totalCount);

            return Result<Contracts.Common.PagedResult<RestaurantDto>>.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogGetAllError(ex);
            return Result<Contracts.Common.PagedResult<RestaurantDto>>.Error(ex.Message);
        }
    }
}
