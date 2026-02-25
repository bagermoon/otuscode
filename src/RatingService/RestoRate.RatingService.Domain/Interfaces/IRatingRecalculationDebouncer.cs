namespace RestoRate.RatingService.Domain.Interfaces;

public interface IRatingRecalculationDebouncer
{
    Task<bool> TryEnterWindowAsync(Guid restaurantId, TimeSpan window, CancellationToken cancellationToken = default);
}
