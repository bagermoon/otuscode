using Ardalis.Result;

using MassTransit;

using Mediator;

using RestoRate.ReviewService.Application.Sagas.Messages;
using RestoRate.ReviewService.Application.UseCases.UserReferences.UpsertUser;

namespace RestoRate.ReviewService.Application.UseCases.UserReferences.UserReferenceValidation;

public sealed class UserReferenceValidationHandler(
	ISender sender,
	IPublishEndpoint publishEndpoint)
	: ICommandHandler<UserReferenceValidationCommand, Result<bool>>
{
	public async ValueTask<Result<bool>> Handle(
		UserReferenceValidationCommand request,
		CancellationToken cancellationToken)
	{
		var upsertResult = await sender.Send(
			new UpsertUserCommand(request.UserId),
			cancellationToken);

		if (!upsertResult.IsOk())
		{
			return Result<bool>.Error(string.Join("; ", upsertResult.Errors));
		}

		var isBlocked = upsertResult.Value;
		var isValid = !isBlocked;

		await publishEndpoint.Publish(
			new UserReferenceValidationStatus(
				UserId: request.UserId,
				IsValid: isValid),
			publishContext => publishContext.CorrelationId = request.UserId,
			cancellationToken);

		return Result<bool>.Success(isValid);
	}
}

