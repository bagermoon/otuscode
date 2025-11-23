using Ardalis.GuardClauses;
using Ardalis.SharedKernel;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;
using RestoRate.Restaurant.Domain.RestaurantAggregate.Events;

namespace RestoRate.Restaurant.Domain.RestaurantAggregate;

public class Restaurant : EntityBase<Guid>, IAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public Address Address { get; private set; } = default!;
    public Location Location { get; private set; } = default!;
    public OpenHours OpenHours { get; private set; } = default!;
    public Money AverageCheck { get; private set; } = default!;

    private readonly List<RestaurantImage> _images = new();
    private readonly List<RestaurantCuisineType> _cuisineTypes = new();
    private readonly List<RestaurantTag> _tags = new();

    public IReadOnlyCollection<RestaurantImage> Images => _images.AsReadOnly();
    public IReadOnlyCollection<RestaurantCuisineType> CuisineTypes => _cuisineTypes.AsReadOnly();
    public IReadOnlyCollection<RestaurantTag> Tags => _tags.AsReadOnly();

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
        Money averageCheck)
    {
        Name = Guard.Against.NullOrEmpty(name, nameof(name));
        Description = Guard.Against.NullOrEmpty(description, nameof(description));
        PhoneNumber = Guard.Against.Null(phoneNumber, nameof(phoneNumber));
        Email = Guard.Against.Null(email, nameof(email));
        Address = Guard.Against.Null(address, nameof(address));
        Location = Guard.Against.Null(location, nameof(location));
        OpenHours = Guard.Against.Null(openHours, nameof(openHours));
        AverageCheck = Guard.Against.Null(averageCheck, nameof(averageCheck));

        RegisterDomainEvent(new RestaurantCreatedEvent(this));
    }

    public void SetPrimaryImage(Guid imageId)
    {
        var targetImage = _images.FirstOrDefault(i => i.Id == imageId);
        Guard.Against.Null(targetImage, nameof(imageId));

        foreach (var img in _images)
            img.UnmarkAsPrimary();

        targetImage.MarkAsPrimary();
        QueueUpdatedEvent();
    }

    public void AddImage(string url, string? altText = null, int displayOrder = 0, bool isPrimary = false)
    {
        if (isPrimary)
            foreach (var img in _images)
                img.UnmarkAsPrimary();

        var image = new RestaurantImage(Id, url, altText, displayOrder, isPrimary);
        _images.Add(image);
        QueueUpdatedEvent();
    }

    public void AddCuisineType(CuisineType cuisineType)
    {
        Guard.Against.Null(cuisineType, nameof(cuisineType));

        if (_cuisineTypes.Any(ct => ct.CuisineType.Equals(cuisineType)))
            return;

        _cuisineTypes.Add(new RestaurantCuisineType(Id, cuisineType));
        QueueUpdatedEvent();
    }

    public void AddTag(SharedKernel.Enums.Tag tag)
    {
        Guard.Against.Null(tag, nameof(tag));

        if (_tags.Any(t => t.Tag.Equals(tag)))
            return;

        _tags.Add(new RestaurantTag(Id, tag));
        QueueUpdatedEvent();
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image != null)
        {
            _images.Remove(image);
            QueueUpdatedEvent();
        }
    }

    public void RemoveCuisineType(Guid cuisineTypeId)
    {
        var cuisineType = _cuisineTypes.FirstOrDefault(ct => ct.Id == cuisineTypeId);
        if (cuisineType != null)
        {
            _cuisineTypes.Remove(cuisineType);
            QueueUpdatedEvent();
        }
    }

    public void RemoveTag(Guid tagId)
    {
        var tag = _tags.FirstOrDefault(t => t.Id == tagId);
        if (tag != null)
        {
            _tags.Remove(tag);
            QueueUpdatedEvent();
        }
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

    public void UpdateAverageCheck(Money averageCheck)
    {
        AverageCheck = Guard.Against.Null(averageCheck, nameof(averageCheck));
        QueueUpdatedEvent();
    }

    public void UpdateCuisineTypes(IEnumerable<CuisineType> cuisineTypes)
    {
        _cuisineTypes.Clear();
        foreach (var ct in cuisineTypes)
            _cuisineTypes.Add(new RestaurantCuisineType(Id, ct));

        QueueUpdatedEvent();
    }

    public void UpdateTags(IEnumerable<SharedKernel.Enums.Tag> tags)
    {
        _tags.Clear();
        foreach (var tag in tags)
            _tags.Add(new RestaurantTag(Id, tag));

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
