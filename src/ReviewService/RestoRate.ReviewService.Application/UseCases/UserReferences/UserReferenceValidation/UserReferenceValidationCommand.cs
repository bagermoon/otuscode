using Ardalis.Result;

using Mediator;

namespace RestoRate.ReviewService.Application.UseCases.UserReferences.UserReferenceValidation;

public sealed record UserReferenceValidationCommand(
	Guid UserId
) : ICommand<Result<bool>>;

