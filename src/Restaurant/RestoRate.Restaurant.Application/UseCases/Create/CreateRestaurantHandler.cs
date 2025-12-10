using Ardalis.Result;
using Ardalis.SharedKernel;
using Mediator;
using Microsoft.Extensions.Logging;
using RestoRate.Abstractions.Messaging;
using RestoRate.Contracts.Restaurant.Events;
using RestoRate.Restaurant.Application.DTOs;
using RestoRate.Restaurant.Domain.Interfaces;
using RestoRate.Restaurant.Domain.RestaurantAggregate;
using RestoRate.Restaurant.Domain.TagAggregate;
using RestoRate.Restaurant.Domain.TagAggregate.Specifications;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;

namespace RestoRate.Restaurant.Application.UseCases.Create;

public sealed class CreateRestaurantHandler(
    IRestaurantService restaurantService,
    IRepository<Tag> tagRepository,
    IIntegrationEventBus integrationEventBus,
    ILogger<CreateRestaurantHandler> logger)
    : ICommandHandler<CreateRestaurantCommand, Result<RestaurantDto>>
{
    public async ValueTask<Result<RestaurantDto>> Handle(
        CreateRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Обработка команды создания ресторана: {RestaurantName}", request.Dto.Name);

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

            var images = request.Dto.Images?
                .Select(img => (img.Url, img.AltText, img.IsPrimary));

            var result = await restaurantService.CreateRestaurant(
                request.Dto.Name,
                request.Dto.Description,
                phoneNumber,
                email,
                address,
                location,
                openHours,
                averageCheck,
                cuisineTypes,
                restaurantTags,
                images);

            if (result.Status != ResultStatus.Ok)
            {
                logger.LogWarning("Не удалось создать ресторан");
                return Result<RestaurantDto>.Error(result.Errors.FirstOrDefault() ?? "Неизвестная ошибка");
            }

            Guid restaurantId = result.Value;

            await integrationEventBus.PublishAsync(new RestaurantCreatedEvent
                (
                    RestaurantId: restaurantId,
                    Name: request.Dto.Name
                ),
                cancellationToken);

            var dto = new RestaurantDto(
                restaurantId,
                request.Dto.Name,
                request.Dto.Description,
                phoneNumber.ToString(),
                email.Address,
                new AddressDto(address.FullAddress, address.House),
                new LocationDto(location.Latitude, location.Longitude),
                new OpenHoursDto(
                    openHours.DayOfWeek,
                    openHours.OpenTime,
                    openHours.CloseTime),
                new MoneyDto(averageCheck.Amount, averageCheck.Currency),
                RestaurantStatus: Status.Draft.Name,
                cuisineTypes.Select(ct => ct.Name).ToList(),
                restaurantTags.Select(t => t.Name).ToList(),
                Array.Empty<RestaurantImageDto>() // изображения тут по сути не нужны
            );

            logger.LogInformation("Ресторан создан успешно: ID {RestaurantId}", result.Value);
            return Result<RestaurantDto>.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при создании ресторана");
            return Result<RestaurantDto>.Error(ex.Message);
        }
    }
}
