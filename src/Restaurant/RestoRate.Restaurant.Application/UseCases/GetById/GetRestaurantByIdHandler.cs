using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;
using RestoRate.Restaurant.Application.DTOs;
using RestoRate.Restaurant.Domain.RestaurantAggregate.Specifications;
using RestoRate.SharedKernel.Interfaces;

namespace RestoRate.Restaurant.Application.UseCases.GetById;

internal class GetRestaurantByIdHandler(
    IReadRepository<Domain.RestaurantAggregate.Restaurant> readRepository,
    ILogger<GetRestaurantByIdHandler> logger)
    : IRequestHandler<GetRestaurantByIdQuery, Result<RestaurantDto>>
{
    public async Task<Result<RestaurantDto>> Handle(
        GetRestaurantByIdQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Обработка запроса получения ресторана: ID {RestaurantId}", request.RestaurantId);

        try
        {
            var spec = new GetRestaurantByIdSpec(request.RestaurantId);
            var restaurant = await readRepository.FirstOrDefaultAsync(spec, cancellationToken);

            if (restaurant == null)
            {
                logger.LogWarning("Ресторан не найден: ID {RestaurantId}", request.RestaurantId);
                return Result.NotFound();
            }

            var dto = new RestaurantDto(
                restaurant.Id,
                restaurant.Name,
                restaurant.Description,
                restaurant.PhoneNumber.ToString(),
                restaurant.Email.Address,
                restaurant.Location.Latitude,
                restaurant.Location.Longitude,
                restaurant.AverageCheck.Amount,
                restaurant.AverageCheck.Currency,
                restaurant.Tag.Name);

            logger.LogInformation("Ресторан найден успешно: ID {RestaurantId}", request.RestaurantId);
            return Result<RestaurantDto>.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при получении ресторана");
            return Result<RestaurantDto>.Error(ex.Message);
        }
    }
}
