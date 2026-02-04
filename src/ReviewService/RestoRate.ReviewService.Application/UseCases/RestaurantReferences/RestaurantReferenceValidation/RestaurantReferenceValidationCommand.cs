using Ardalis.Result;

using Mediator;

using RestoRate.Contracts.Restaurant;

namespace RestoRate.ReviewService.Application.UseCases.RestaurantReferences.RestaurantReferenceValidation;

public record RestaurantReferenceValidationCommand(
    Guid RestaurantId,
    RestaurantStatus? KnownStatus = null,
    bool? Exists = null
) : ICommand<Result<bool>>;
