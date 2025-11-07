using Ardalis.GuardClauses;
using Ardalis.SharedKernel;

namespace RestoRate.SharedKernel.ValueObjects;

public class Location : ValueObject
{
    /// <summary> Широта (-90 до 90 градусов) </summary>
    public double Latitude { get; }

    /// <summary> Долгота (-180 до 180 градусов) </summary>
    public double Longitude { get; }

    public Location(double latitude, double longitude)
    {
        Latitude = Guard.Against.OutOfRange(latitude, nameof(latitude), -90.0, 90.0);
        Longitude = Guard.Against.OutOfRange(longitude, nameof(longitude), -180.0, 180.0);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }

    public override string ToString() => $"{Latitude:F6}, {Longitude:F6}";
}
