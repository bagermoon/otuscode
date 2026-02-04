namespace RestoRate.Contracts.Review.Dtos;

public record UserReferenceDto
(
    Guid UserId,
    string? Name,
    string? FullName,
    string? Email,
    bool IsBlocked
)
{ }
