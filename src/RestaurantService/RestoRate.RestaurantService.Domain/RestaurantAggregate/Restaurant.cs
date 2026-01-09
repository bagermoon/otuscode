using Ardalis.GuardClauses;
using Ardalis.SharedKernel;
using RestoRate.SharedKernel.Enums;
using RestoRate.SharedKernel.ValueObjects;
using RestoRate.RestaurantService.Domain.RestaurantAggregate.Events;
using RestoRate.RestaurantService.Domain.TagAggregate;
using Ardalis.Result;

namespace RestoRate.RestaurantService.Domain.RestaurantAggregate;

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
    public RestaurantStatus RestaurantStatus { get; private set; } = RestaurantStatus.Draft;

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
        RestaurantStatus = RestaurantStatus.Draft;
    }

    public static Restaurant Create(
        string name,
        string description,
        PhoneNumber phoneNumber,
        Email email,
        Address address,
        Location location,
        OpenHours openHours,
        Money averageCheck
    )
    {
        var restaurant = new Restaurant(
            name,
            description,
            phoneNumber,
            email,
            address,
            location,
            openHours,
            averageCheck);

        restaurant.RegisterDomainEvent(new RestaurantCreatedEvent(restaurant));

        return restaurant;
    }

    public Restaurant AddImages(IEnumerable<(string Url, string? AltText, bool IsPrimary)>? images)
    {
        if (images == null)
            return this;

        int displayOrder = 0;
        foreach (var (url, altText, isPrimary) in images)
            AddImage(url, altText, displayOrder++, isPrimary);
        return this;
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

    public Restaurant AddCuisineTypes(IEnumerable<CuisineType> cuisineTypes)
    {
        foreach (var ct in cuisineTypes)
            AddCuisineType(ct);

        return this;
    }

    public void AddCuisineType(CuisineType cuisineType)
    {
        Guard.Against.Null(cuisineType, nameof(cuisineType));

        if (_cuisineTypes.Any(ct => ct.CuisineType.Equals(cuisineType)))
            return;

        _cuisineTypes.Add(new RestaurantCuisineType(Id, cuisineType));
    }

    public Restaurant AddTags(IEnumerable<Tag> tags)
    {
        foreach (var tag in tags)
            AddTag(tag);

        return this;
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

    public Result SendToModeration()
    {
        if (RestaurantStatus != RestaurantStatus.Draft && RestaurantStatus != RestaurantStatus.Rejected)
            return Result.Error("Only draft or rejected restaurants can be sent to moderation.");

        RestaurantStatus = RestaurantStatus.OnModeration;
        RegisterDomainEvent(new RestaurantUpdatedEvent(this));

        return Result.Success();
    }

    public Result Publish()
    {
        if (RestaurantStatus != RestaurantStatus.OnModeration)
            return Result.Error("Only restaurants under moderation can be published.");

        RestaurantStatus = RestaurantStatus.Published;
        RegisterDomainEvent(new RestaurantCreatedEvent(this));

        return Result.Success();
    }

    public Result Reject()
    {
        if (RestaurantStatus != RestaurantStatus.OnModeration)
            return Result.Error("Only restaurants under moderation can be rejected.");

        RestaurantStatus = RestaurantStatus.Rejected;
        RegisterDomainEvent(new RestaurantUpdatedEvent(this));

        return Result.Success();
    }

    public Result MarkDeleted()
    {
        if (RestaurantStatus == RestaurantStatus.Archived)
            return Result.Error("Restaurant is already deleted.");

        RestaurantStatus = RestaurantStatus.Archived;

        if (!_deletedEventQueued)
        {
            RegisterDomainEvent(new RestaurantDeletedEvent(this));
            _deletedEventQueued = true;
        }

        return Result.Success();
    }

    public Result UpdateStatus(RestaurantStatus status)
    {
        if (RestaurantStatus.IsDeleted())
            return Result.Error("Cannot change status of a deleted restaurant.");

        return status.Name switch
        {
            nameof(RestaurantStatus.Published) => Publish(),
            nameof(RestaurantStatus.Rejected) => Reject(),
            nameof(RestaurantStatus.OnModeration) => SendToModeration(),
            _ => Result.Error($"Invalid status transition to {status.Name}"),
        };
    }

    public new void ClearDomainEvents()
    {
        base.ClearDomainEvents();
        _updatedEventQueued = false;
        _deletedEventQueued = false;
    }
}
