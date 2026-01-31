using Ardalis.SharedKernel;

namespace RestoRate.ReviewService.Domain.UserReferenceAggregate;

public class UserReference : EntityBase<Guid>, IAggregateRoot
{
    public bool IsBlocked { get; private set; }

    private UserReference() { }

    private UserReference(Guid userId, bool isBlocked = false)
    {
        if (userId == Guid.Empty) throw new ArgumentException("userId must be set", nameof(userId));
        Id = userId;
        IsBlocked = isBlocked;
    }

    public static UserReference Create(Guid userId, bool isBlocked = false)
        => new UserReference(userId, isBlocked);

    public UserReference SetBlocked(bool blocked)
    {
        IsBlocked = blocked;
        return this;
    }
}
