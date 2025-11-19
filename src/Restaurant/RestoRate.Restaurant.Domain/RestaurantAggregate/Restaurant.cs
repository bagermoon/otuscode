using Ardalis.GuardClauses;
using Ardalis.SharedKernel;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;

namespace RestoRate.Restaurant.Domain.RestaurantAggregate;

public class Restaurant : EntityBase, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public Address Address { get; private set; } = default!;
    public Location Location { get; private set; } = default!;
    public OpenHours OpenHours { get; private set; } = default!;
    public CuisineType CuisineType { get; private set; } = default!;
    public Money AverageCheck { get; private set; } = default!;
    public RestaurantTag Tag { get; private set; } = default!;

    private Restaurant() { }
    public Restaurant(
        string name,
        string description,
        PhoneNumber phoneNumber,
        Email email,
        Address address,
        Location location,
        OpenHours openHours,
        CuisineType cuisineType,
        Money averageCheck,
        RestaurantTag tag)
    {
        Name = Guard.Against.NullOrEmpty(name, nameof(name));
        Description = Guard.Against.NullOrEmpty(description, nameof(description));
        PhoneNumber = Guard.Against.Null(phoneNumber, nameof(phoneNumber));
        Email = Guard.Against.Null(email, nameof(email));
        Address = Guard.Against.Null(address, nameof(address));
        Location = Guard.Against.Null(location, nameof(location));
        OpenHours = Guard.Against.Null(openHours, nameof(openHours));
        CuisineType = Guard.Against.Null(cuisineType, nameof(cuisineType));
        AverageCheck = Guard.Against.Null(averageCheck, nameof(averageCheck));
        Tag = Guard.Against.Null(tag, nameof(tag));
    }

    public void UpdateName(string name) => Name = Guard.Against.NullOrEmpty(name, nameof(name));
    public void UpdateDescription(string description) => Description = Guard.Against.NullOrEmpty(description, nameof(description));
    public void UpdatePhoneNumber(PhoneNumber phoneNumber) => PhoneNumber = Guard.Against.Null(phoneNumber, nameof(phoneNumber));
    public void UpdateEmail(Email email) => Email = Guard.Against.Null(email, nameof(email));
    public void UpdateAddress(Address address) => Address = Guard.Against.Null(address, nameof(address));
    public void UpdateLocation(Location location) => Location = Guard.Against.Null(location, nameof(location));
    public void UpdateOpenHours(OpenHours openHours) => OpenHours = Guard.Against.Null(openHours, nameof(openHours));
    public void UpdateCuisineType(CuisineType cuisineType) => CuisineType = Guard.Against.Null(cuisineType, nameof(cuisineType));
    public void UpdateAverageCheck(Money averageCheck) =>  AverageCheck = Guard.Against.Null(averageCheck, nameof(averageCheck));
    public void UpdateTag(RestaurantTag tag) => Tag = Guard.Against.Null(tag, nameof(tag));
}
