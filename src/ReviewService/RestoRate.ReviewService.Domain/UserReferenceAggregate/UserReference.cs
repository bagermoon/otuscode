using Ardalis.SharedKernel;

using RestoRate.SharedKernel.ValueObjects;

namespace RestoRate.ReviewService.Domain.UserReferenceAggregate;

public class UserReference : EntityBase<Guid>, IAggregateRoot
{
    public bool IsBlocked { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public Email? Email { get; private set; }

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

    public UserReference SetName(string name)
    {
        Name = name;
        return this;
    }

    public UserReference SetFullName(string fullName)
    {
        FullName = fullName;
        return this;
    }
    public UserReference SetEmail(Email email)
    {
        Email = email;
        return this;
    }
}
