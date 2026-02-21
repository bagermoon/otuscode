using MongoDB.Driver;

using RestoRate.Abstractions.Persistence;

namespace RestoRate.RatingService.Infrastructure.Data;

public interface ISessionHolder : ISessionContext, IDisposable
{
    /// <summary>
    /// Возвращает активную сессию Mongo, если она доступна/необходима.
    ///
    /// Lazy семантика:
    /// - <see cref="ISessionContext.BeginTransactionAsync"/> может только запросить транзакцию.
    /// - Реализация может начать фактическую сессию/транзакцию Mongo при первом вызове этого метода.
    /// - Если транзакции/сессии отключены/не поддерживаются, возвращает null.
    /// </summary>
    ValueTask<IClientSessionHandle?> GetSessionAsync(CancellationToken cancellationToken = default);
}
