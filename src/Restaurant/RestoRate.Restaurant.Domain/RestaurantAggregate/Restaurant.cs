using Ardalis.GuardClauses;
using Ardalis.SharedKernel;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;

namespace RestoRate.Restaurant.Domain.RestaurantAggregate;

public class Restaurant : EntityBase, IAggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public Email Email { get; private set; }
    public Location Location { get; private set; }
    public Money AverageCheck { get; private set; }
    public RestaurantTag Tag { get; private set; }

    public Restaurant(
        string name,
        string description,
        PhoneNumber phoneNumber,
        Email email,
        Location location,
        Money averageCheck,
        RestaurantTag tag)
    {
        Name = Guard.Against.NullOrEmpty(name, nameof(name));
        Description = Guard.Against.NullOrEmpty(description, nameof(description));
        PhoneNumber = Guard.Against.Null(phoneNumber, nameof(phoneNumber));
        Email = Guard.Against.Null(email, nameof(email));
        Location = Guard.Against.Null(location, nameof(location));
        AverageCheck = Guard.Against.Null(averageCheck, nameof(averageCheck));
        Tag = Guard.Against.Null(tag, nameof(tag));
    }

    public void UpdateName(string name) => Name = Guard.Against.NullOrEmpty(name, nameof(name));
    public void UpdateDescription(string description) => Description = Guard.Against.NullOrEmpty(description, nameof(description));
    public void UpdatePhoneNumber(PhoneNumber phoneNumber) => PhoneNumber = Guard.Against.Null(phoneNumber, nameof(phoneNumber));
    public void UpdateEmail(Email email) => Email = Guard.Against.Null(email, nameof(email));
    public void UpdateLocation(Location location) => Location = Guard.Against.Null(location, nameof(location));
    public void UpdateAverageCheck(Money averageCheck) =>  AverageCheck = Guard.Against.Null(averageCheck, nameof(averageCheck));
    public void UpdateTag(RestaurantTag tag) => Tag = Guard.Against.Null(tag, nameof(tag));
}
