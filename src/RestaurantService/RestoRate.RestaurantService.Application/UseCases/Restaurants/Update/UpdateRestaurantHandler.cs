using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using Microsoft.Extensions.Logging;

using RestoRate.RestaurantService.Domain.Interfaces;
using RestoRate.RestaurantService.Domain.TagAggregate;
using RestoRate.RestaurantService.Domain.TagAggregate.Specifications;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.Update;

public sealed class UpdateRestaurantHandler(
    IRestaurantService restaurantService,
    IRepository<Tag> tagRepository,
    ILogger<UpdateRestaurantHandler> logger)
    : ICommandHandler<UpdateRestaurantCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Обработка команды обновления ресторана: ID {RestaurantId}", request.Dto.RestaurantId);

        try
        {
            var phoneNumber = new PhoneNumber("+7", request.Dto.PhoneNumber);
            var email = new Email(request.Dto.Email);
            var address = new Address(request.Dto.Address.FullAddress, request.Dto.Address.House);
            var location = new Location(request.Dto.Location.Latitude, request.Dto.Location.Longitude);
            var openHours = new OpenHours(request.Dto.OpenHours.DayOfWeek, request.Dto.OpenHours.OpenTime, request.Dto.OpenHours.CloseTime);
            var averageCheck = new Money(request.Dto.AverageCheck.Amount, request.Dto.AverageCheck.Currency);

            var cuisineTypes = request.Dto.CuisineTypes
                .Select(ct => CuisineType.FromName(ct))
                .ToList();

            var restaurantTags = new List<Tag>();
            if (request.Dto.Tags != null && request.Dto.Tags.Count != 0)
            {
                var uniqueTags = request.Dto.Tags.Distinct(StringComparer.OrdinalIgnoreCase);

                foreach (var tagName in uniqueTags)
                {
                    var spec = new TagByNameSpec(tagName);
                    var existingTag = await tagRepository.FirstOrDefaultAsync(spec, cancellationToken);

                    if (existingTag != null)
                    {
                        restaurantTags.Add(existingTag);
                    }
                    else
                    {
                        var newTag = new Tag(tagName);
                        await tagRepository.AddAsync(newTag, cancellationToken);
                        restaurantTags.Add(newTag);
                    }
                }
            }

            var result = await restaurantService.UpdateRestaurant(
                request.Dto.RestaurantId,
                request.Dto.Name,
                request.Dto.Description,
                phoneNumber,
                email,
                address,
                location,
                openHours,
                averageCheck,
                cuisineTypes,
                restaurantTags);

            if (result.Status == ResultStatus.NotFound)
            {
                logger.LogWarning("Ресторан не найден: ID {RestaurantId}", request.Dto.RestaurantId);
                return Result.NotFound();
            }

            if (result.Status != ResultStatus.Ok)
            {
                logger.LogWarning("Не удалось обновить ресторан");
                return Result.Error(result.Errors.FirstOrDefault() ?? "Неизвестная ошибка");
            }

            logger.LogInformation("Ресторан обновлен успешно: ID {RestaurantId}", request.Dto.RestaurantId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при обновлении ресторана");
            return Result.Error(ex.Message);
        }
    }
}
