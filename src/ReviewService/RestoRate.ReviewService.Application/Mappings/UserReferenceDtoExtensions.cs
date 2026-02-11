using RestoRate.Contracts.Review.Dtos;
using RestoRate.ReviewService.Domain.UserReferenceAggregate;

namespace RestoRate.ReviewService.Application.Mappings;

public static class UserReferenceDtoExtensions
{
    public static UserReferenceDto ToDto(this UserReference? reference)
    {
        if (reference is null)
        {
            return new UserReferenceDto(
                UserId: Guid.Empty,
                Name: null,
                FullName: null,
                Email: null,
                IsBlocked: false
            );
        }

        return new UserReferenceDto(
            UserId: reference.Id,
            Name: reference.Name,
            FullName: reference.FullName,
            Email: reference.Email?.ToString(),
            IsBlocked: reference.IsBlocked
        );
    }
}
