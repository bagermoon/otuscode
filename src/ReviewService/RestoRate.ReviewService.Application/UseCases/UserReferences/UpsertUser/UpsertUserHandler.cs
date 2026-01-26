using Ardalis.Result;
using Ardalis.SharedKernel;

using Mediator;

using Microsoft.Extensions.Logging;

using RestoRate.ReviewService.Domain.UserReferenceAggregate;

namespace RestoRate.ReviewService.Application.UseCases.UserReferences.UpsertUser;

public sealed class UpsertUserHandler(
	IRepository<UserReference> repository,
	ILogger<UpsertUserHandler> logger)
	: ICommandHandler<UpsertUserCommand, Result<bool>>
{
	public async ValueTask<Result<bool>> Handle(
		UpsertUserCommand request,
		CancellationToken cancellationToken)
	{
		if (request.UserId == Guid.Empty)
		{
            return Result<bool>.Success(false);
        }

		var userReference = await repository.GetByIdAsync(request.UserId, cancellationToken);

		if (userReference is not null)
		{
			if (request.IsBlocked.HasValue && userReference.IsBlocked != request.IsBlocked.Value)
			{
				userReference.SetBlocked(request.IsBlocked.Value);
				await repository.UpdateAsync(userReference, cancellationToken);
			}

			return Result<bool>.Success(userReference.IsBlocked);
		}

		var newReference = UserReference.Create(
			request.UserId,
			isBlocked: request.IsBlocked ?? false);

		try
		{
			await repository.AddAsync(newReference, cancellationToken);
			return Result<bool>.Success(newReference.IsBlocked);
		}
		catch (Exception ex) when (LooksLikeAlreadyExists(ex))
		{
			logger.LogUserReferenceAlreadyExists(request.UserId);

			var existing = await repository.GetByIdAsync(request.UserId, cancellationToken);
			if (existing is not null)
			{
				return Result<bool>.Success(existing.IsBlocked);
			}

			return Result<bool>.Error(
				$"UserReference insert raced for {request.UserId}, but could not be reloaded.");
		}
	}

	private static bool LooksLikeAlreadyExists(Exception ex)
	{
		for (var current = ex; current is not null; current = current.InnerException)
		{
			var message = current.Message;
			if (!string.IsNullOrWhiteSpace(message) &&
				(message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
				 message.Contains("E11000", StringComparison.OrdinalIgnoreCase) ||
				 message.Contains("already exists", StringComparison.OrdinalIgnoreCase)))
			{
				return true;
			}
		}

		return false;
	}
}

