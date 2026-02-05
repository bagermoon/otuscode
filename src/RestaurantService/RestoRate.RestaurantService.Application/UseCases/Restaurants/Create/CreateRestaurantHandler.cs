using Ardalis.Result;

using Mediator;

using Microsoft.Extensions.Logging;

using NodaMoney;

using RestoRate.Abstractions.Identity;
using RestoRate.Contracts.Restaurant.DTOs;
using RestoRate.RestaurantService.Application.Mappings;
using RestoRate.RestaurantService.Domain.Interfaces;
using RestoRate.RestaurantService.Domain.RestaurantAggregate;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;

namespace RestoRate.RestaurantService.Application.UseCases.Restaurants.Create;

public sealed class CreateRestaurantHandler(
    IRestaurantService restaurantService,
    ITagsService tagsService,
    IUserContext userContext,
    ILogger<CreateRestaurantHandler> logger)
    : ICommandHandler<CreateRestaurantCommand, Result<RestaurantDto>>
{
    public async ValueTask<Result<RestaurantDto>> Handle(
        CreateRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogCreating(request.Dto.Name);

        try
        {
            var phoneNumber = new PhoneNumber("+7", request.Dto.PhoneNumber);
            var email = new Email(request.Dto.Email);
            var address = new Address(request.Dto.Address.FullAddress, request.Dto.Address.House);
            var location = new Location(request.Dto.Location.Latitude, request.Dto.Location.Longitude);

            var openHours = request.Dto.OpenHours
                .Select(oh => new OpenHours(oh.DayOfWeek, oh.OpenTime, oh.CloseTime, oh.IsClosed))
                .ToList();

            var averageCheck = new Money(request.Dto.AverageCheck.Amount, Currency.FromCode(request.Dto.AverageCheck.Currency));

            var cuisineTypes = request.Dto.CuisineTypes
                .Select(ct => CuisineType.FromName(ct))
                .ToList();

            var restaurantTags = await tagsService.ConvertToTagsAsync(
                request.Dto.Tags ?? [], cancellationToken);

            var images = request.Dto.Images?
                .Select(img => (img.Url, img.AltText, img.IsPrimary));

            var result = await restaurantService.CreateRestaurantAsync(
                request.Dto.Name,
                request.Dto.Description ?? string.Empty,
                phoneNumber,
                email,
                address,
                location,
                openHours,
                averageCheck,
                userContext.UserId,
                cuisineTypes,
                restaurantTags,
                images);

            if (result.Status != ResultStatus.Ok)
            {
                logger.LogCreateFailed();
                return Result<RestaurantDto>.Error(result.Errors.FirstOrDefault() ?? "Неизвестная ошибка");
            }

            Restaurant restaurant = result.Value;

            var dto = restaurant.ToDto();

            logger.LogCreated(result.Value.Id);
            return Result<RestaurantDto>.Created(dto);
        }
        catch (Exception ex)
        {
            logger.LogCreateError(ex);
            return Result<RestaurantDto>.Error(ex.Message);
        }
    }
}
