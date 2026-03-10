namespace RestoRate.RatingService.Domain.Interfaces;

public interface IRatingRecalculationDebouncer
{
    Task MarkChangedAsync(Guid restaurantId, TimeSpan window, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetDueRestaurantIdsAsync(
        DateTimeOffset asOf,
        int take,
        CancellationToken cancellationToken = default);

    Task<DateTimeOffset?> GetDueAtAsync(Guid restaurantId, CancellationToken cancellationToken = default);

    Task<bool> TryAcquireProcessingLockAsync(
        Guid restaurantId,
        TimeSpan lockDuration,
        CancellationToken cancellationToken = default);

    Task<bool> TryCompleteAsync(
        Guid restaurantId,
        DateTimeOffset expectedDueAt,
        CancellationToken cancellationToken = default);

    Task ReleaseProcessingLockAsync(Guid restaurantId, CancellationToken cancellationToken = default);
}
