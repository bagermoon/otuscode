using Ardalis.Result;

using Mediator;

namespace RestoRate.ReviewService.Application.UseCases.UserReferences.UpsertUser;

/// <summary>
/// Upserts the local <see cref="RestoRate.ReviewService.Domain.UserReferenceAggregate.UserReference"/> cache entry.
/// Returns the current <c>IsBlocked</c> value.
/// </summary>
public sealed record UpsertUserCommand(
    Guid UserId,
    bool? IsBlocked = null
) : ICommand<Result<bool>>;

