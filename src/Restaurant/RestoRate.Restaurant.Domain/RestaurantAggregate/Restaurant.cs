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
    public Location Location { get; private set; } = default!;
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

    public void UpdateLocation(Location location)
    {
        Location = Guard.Against.Null(location, nameof(location));
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
