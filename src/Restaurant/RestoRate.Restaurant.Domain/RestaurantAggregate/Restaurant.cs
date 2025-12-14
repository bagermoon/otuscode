using Ardalis.GuardClauses;
using Ardalis.SharedKernel;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;
using RestoRate.Restaurant.Domain.RestaurantAggregate.Events;
using RestoRate.Restaurant.Domain.TagAggregate;

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
    public Status RestaurantStatus { get; private set; } = Status.Draft;

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
        Id = Guid.NewGuid();
        Name = Guard.Against.NullOrEmpty(name, nameof(name));
        Description = Guard.Against.NullOrEmpty(description, nameof(description));
        PhoneNumber = Guard.Against.Null(phoneNumber, nameof(phoneNumber));
        Email = Guard.Against.Null(email, nameof(email));
        Address = Guard.Against.Null(address, nameof(address));
        Location = Guard.Against.Null(location, nameof(location));
        OpenHours = Guard.Against.Null(openHours, nameof(openHours));
        AverageCheck = Guard.Against.Null(averageCheck, nameof(averageCheck));
        RestaurantStatus = Status.Draft;

        RegisterDomainEvent(new RestaurantCreatedEvent(this));
    }

    public RestaurantImage AddImage(string url, string? altText = null, int displayOrder = 0, bool isPrimary = false)
    {
        if (isPrimary)
            foreach (var img in _images)
                img.UnmarkAsPrimary();

        var image = new RestaurantImage(Id, url, altText, displayOrder, isPrimary);
        _images.Add(image);

        return image;
    }

    public void SetPrimaryImage(Guid imageId)
    {
        var targetImage = _images.FirstOrDefault(i => i.Id == imageId);
        Guard.Against.Null(targetImage, nameof(imageId));

        foreach (var img in _images)
            img.UnmarkAsPrimary();

        targetImage.MarkAsPrimary();
    }

    public void AddCuisineType(CuisineType cuisineType)
    {
        Guard.Against.Null(cuisineType, nameof(cuisineType));

        if (_cuisineTypes.Any(ct => ct.CuisineType.Equals(cuisineType)))
            return;

        _cuisineTypes.Add(new RestaurantCuisineType(Id, cuisineType));
    }

    public void AddTag(Tag tag)
    {
        Guard.Against.Null(tag, nameof(tag));

        if (_tags.Any(t => t.TagId.Equals(tag.Id)))
            return;

        _tags.Add(new RestaurantTag(Id, tag.Id));
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image != null)
        {
            _images.Remove(image);
        }
    }

    public void RemoveCuisineType(Guid cuisineTypeId)
    {
        var cuisineType = _cuisineTypes.FirstOrDefault(ct => ct.Id == cuisineTypeId);
        if (cuisineType != null)
            _cuisineTypes.Remove(cuisineType);
    }

    public void RemoveTag(Guid tagId)
    {
        var tag = _tags.FirstOrDefault(t => t.Id == tagId);
        if (tag != null)
            _tags.Remove(tag);
    }

    public void UpdateName(string name)
    {
        Name = Guard.Against.NullOrEmpty(name, nameof(name));
    }

    public void UpdateDescription(string description)
    {
        Description = Guard.Against.NullOrEmpty(description, nameof(description));
    }

    public void UpdatePhoneNumber(PhoneNumber phoneNumber)
    {
        PhoneNumber = Guard.Against.Null(phoneNumber, nameof(phoneNumber));
    }

    public void UpdateEmail(Email email)
    {
        Email = Guard.Against.Null(email, nameof(email));
    }

    public void UpdateAddress(Address address)
    {
        Address = Guard.Against.Null(address, nameof(address));
    }

    public void UpdateLocation(Location location)
    {
        Location = Guard.Against.Null(location, nameof(location));
    }

    public void UpdateOpenHours(OpenHours openHours)
    {
        OpenHours = Guard.Against.Null(openHours, nameof(openHours));
    }

    public void UpdateAverageCheck(Money averageCheck)
    {
        AverageCheck = Guard.Against.Null(averageCheck, nameof(averageCheck));
    }

    public void UpdateCuisineTypes(IEnumerable<CuisineType> cuisineTypes)
    {
        _cuisineTypes.Clear();
        foreach (var ct in cuisineTypes)
            _cuisineTypes.Add(new RestaurantCuisineType(Id, ct));
    }

    public void UpdateTags(IEnumerable<Tag> tags)
    {
        Guard.Against.Null(tags, nameof(tags));

        var newTagsList = tags.ToList();

        var tagsToRemove = _tags
            .Where(existing => !newTagsList.Any(nt => nt.Id == existing.TagId))
            .ToList();

        foreach (var tagToRemove in tagsToRemove)
            _tags.Remove(tagToRemove);

        foreach (var newTag in newTagsList)
            if (!_tags.Any(existing => existing.TagId == newTag.Id))
                _tags.Add(new RestaurantTag(Id, newTag.Id));
    }

    public void SendToModeration()
    {
        if (RestaurantStatus != Status.Draft && RestaurantStatus != Status.Rejected)
            return;

        RestaurantStatus = Status.OnModeration;
        RegisterDomainEvent(new RestaurantUpdatedEvent(this));
    }

    public void Publish()
    {
        if (RestaurantStatus != Status.OnModeration)
            return;

        RestaurantStatus = Status.Published;
        RegisterDomainEvent(new RestaurantUpdatedEvent(this));
    }

    public void Reject()
    {
        if (RestaurantStatus != Status.OnModeration)
            return;

        RestaurantStatus = Status.Rejected;
        RegisterDomainEvent(new RestaurantUpdatedEvent(this));
    }

    public void MarkDeleted()
    {
        if (RestaurantStatus == Status.Archived)
            return;

        RestaurantStatus = Status.Archived;

        if (!_deletedEventQueued)
        {
            RegisterDomainEvent(new RestaurantDeletedEvent(this));
            _deletedEventQueued = true;
        }
    }

    public void Restore()
    {
        if (RestaurantStatus != Status.Archived)
            return;

        RestaurantStatus = Status.Draft;
        RegisterDomainEvent(new RestaurantUpdatedEvent(this));
    }

    public new void ClearDomainEvents()
    {
        base.ClearDomainEvents();
        _updatedEventQueued = false;
        _deletedEventQueued = false;
    }
}
