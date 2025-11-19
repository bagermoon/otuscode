using Ardalis.GuardClauses;
using Ardalis.SharedKernel;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;
using RestoRate.Restaurant.Domain.RestaurantAggregate.Events;

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

    private bool _updatedEventQueued;
    private bool _deletedEventQueued;

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

        RegisterDomainEvent(new RestaurantCreatedEvent(this));
    }
    public void UpdateName(string name)
    {
        Name = Guard.Against.NullOrEmpty(name, nameof(name));
        QueueUpdatedEvent();
    }

    public void UpdateDescription(string description)
    {
        Description = Guard.Against.NullOrEmpty(description, nameof(description));
        QueueUpdatedEvent();
    }

    public void UpdatePhoneNumber(PhoneNumber phoneNumber)
    {
        PhoneNumber = Guard.Against.Null(phoneNumber, nameof(phoneNumber));
        QueueUpdatedEvent();
    }

    public void UpdateEmail(Email email)
    {
        Email = Guard.Against.Null(email, nameof(email));
        QueueUpdatedEvent();
    }

    public void UpdateAddress(Address address)
    {
        Address = Guard.Against.Null(address, nameof(address));
        QueueUpdatedEvent();
    }

    public void UpdateLocation(Location location)
    {
        Location = Guard.Against.Null(location, nameof(location));
        QueueUpdatedEvent();
    }

    public void UpdateOpenHours(OpenHours openHours)
    {
        OpenHours = Guard.Against.Null(openHours, nameof(openHours));
        QueueUpdatedEvent();
    }

    public void UpdateCuisineType(CuisineType cuisineType)
    {
        CuisineType = Guard.Against.Null(cuisineType, nameof(cuisineType));
        QueueUpdatedEvent();
    }

    public void UpdateAverageCheck(Money averageCheck)
    {
        AverageCheck = Guard.Against.Null(averageCheck, nameof(averageCheck));
        QueueUpdatedEvent();
    }

    public void UpdateTag(RestaurantTag tag)
    {
        Tag = Guard.Against.Null(tag, nameof(tag));
        QueueUpdatedEvent();
    }

    public void MarkDeleted()
    {
        if (_deletedEventQueued)
        {
            return;
        }

        RegisterDomainEvent(new RestaurantDeletedEvent(this));
        _deletedEventQueued = true;
    }

    public new void ClearDomainEvents()
    {
        base.ClearDomainEvents();
        _updatedEventQueued = false;
        _deletedEventQueued = false;
    }

    private void QueueUpdatedEvent()
    {
        if (_updatedEventQueued)
        {
            return;
        }

        RegisterDomainEvent(new RestaurantUpdatedEvent(this));
        _updatedEventQueued = true;
    }
}
