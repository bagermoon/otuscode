using Ardalis.SharedKernel;

using MongoDB.Driver;

using Microsoft.Extensions.Logging;

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace RestoRate.RatingService.Infrastructure.Data;

/// <summary>
/// Состояние агрегата для отслеживания изменений в рамках Unit of Work.
/// </summary>
public enum EntityState
{
    New,
    Dirty,
    Deleted,
    Unchanged // для прочитанных
}

/// <summary>
/// Unit of Work для MongoDB, который отслеживает агрегаты и их состояния (новые, измененные, удаленные, неизменные).
/// </summary>
public class MongoUnitOfWork : IUnitOfWork
{
    // 0 = unknown, 1 = supported, -1 = not supported (e.g., standalone Mongo).
    private static int _transactionsSupportedState;

    private readonly IMongoDatabase _database;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly ILogger<MongoUnitOfWork> _logger;
    private readonly ReadOnlyDictionary<Type, IMongoAggregateWriter> _writers;
    private readonly Dictionary<string, TrackedEntity> _trackedEntities;

    public MongoUnitOfWork(
        IMongoDatabase database,
        IEnumerable<IMongoAggregateWriter> writers,
        IDomainEventDispatcher domainEventDispatcher,
        ILogger<MongoUnitOfWork> logger
    )
    {
        _database = database;
        _domainEventDispatcher = domainEventDispatcher;
        _logger = logger;
        _writers = new ReadOnlyDictionary<Type, IMongoAggregateWriter>(
            (writers ?? throw new ArgumentNullException(nameof(writers)))
            .GroupBy(x => x.DocumentType)
            .ToDictionary(g => g.Key, g =>
            {
                if (g.Count() > 1)
                {
                    throw new InvalidOperationException($"Multiple IMongoAggregateWriter registrations found for type '{g.Key.FullName}'.");
                }
                return g.Single();
            }));
        _trackedEntities = new Dictionary<string, TrackedEntity>();
    }

    private sealed class TrackedEntity(EntityBase<Guid> aggregate, EntityState state = EntityState.Unchanged)
    {
        public EntityBase<Guid> Aggregate { get; } = aggregate;
        public EntityState State { get; set; } = state;
    }

    public bool TryGet<TAggregate>(Guid id, out TAggregate aggregate)
        where TAggregate : EntityBase<Guid>
    {
        if (_trackedEntities.TryGetValue(GetKey(typeof(TAggregate), id), out var tracked))
        {
            aggregate = (TAggregate)tracked.Aggregate;
            return true;
        }

        aggregate = default!;
        return false;
    }

    public void Attach<TAggregate>(TAggregate aggregate, EntityState state = EntityState.Unchanged)
        where TAggregate : EntityBase<Guid>
    {
        var key = GetKey(typeof(TAggregate), aggregate.Id);

        if (_trackedEntities.TryGetValue(key, out var existing))
        {
            if (!ReferenceEquals(existing.Aggregate, aggregate))
            {
                throw new InvalidOperationException($"Aggregate with id {aggregate.Id} already exists");
            }

            existing.State = state;

            return;
        }

        _trackedEntities[key] = new TrackedEntity(aggregate, state);
    }

    public void MarkNew<TAggregate>(TAggregate aggregate)
        where TAggregate : EntityBase<Guid>
    {
        Attach(aggregate, EntityState.New);
    }

    public void MarkDirty<TAggregate>(TAggregate aggregate)
        where TAggregate : EntityBase<Guid>
    {
        Attach(aggregate, EntityState.Dirty);
    }

    public void MarkDeleted<TAggregate>(TAggregate aggregate)
        where TAggregate : EntityBase<Guid>
    {
        Attach(aggregate, EntityState.Deleted);
    }

    public bool HasChanges() => _trackedEntities.Values.Any(x =>
        x.State != EntityState.Unchanged || x.Aggregate.DomainEvents.Count > 0);

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        if (!HasChanges()) return true;

        // If transactions are known to be unsupported (standalone Mongo), avoid creating sessions entirely.
        if (Volatile.Read(ref _transactionsSupportedState) < 0)
        {
            return await SaveEntitiesWithoutSessionAsync(cancellationToken);
        }

        using var session = await _database.Client.StartSessionAsync(cancellationToken: cancellationToken);

        return await SaveEntitiesWithManagedSessionAsync(session, cancellationToken);
    }

    public Task<bool> SaveEntitiesAsync(IClientSessionHandle session, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session, nameof(session));
        if (!HasChanges()) return Task.FromResult(true);

        // External session is typically managed by the caller (e.g., MassTransit MongoDbContext.Session)
        // and may already have an active transaction.
        return SaveEntitiesWithExternalSessionAsync(session, cancellationToken);
    }

    private async Task<bool> SaveEntitiesWithManagedSessionAsync(
        [NotNull] IClientSessionHandle session,
        CancellationToken cancellationToken)
    {
        if (session.IsInTransaction)
        {
            throw new InvalidOperationException(
                "MongoUnitOfWork cannot manage a transaction for a session that is already in a transaction. " +
                "Call SaveEntitiesAsync(session, ...) when the transaction is managed externally.");
        }

        try
        {
            var cachedState = Volatile.Read(ref _transactionsSupportedState);
            if (cachedState < 0)
            {
                // Transactions are known to be unsupported. Avoid session usage entirely.
                await SaveChangesAsync(session: null, cancellationToken);
            }
            else
            {
                // Try transaction first; on first unsupported error cache the result and fall back.
                session.StartTransaction();
                try
                {
                    await SaveChangesAsync(session, cancellationToken);
                    await session.CommitTransactionAsync(cancellationToken);
                    Interlocked.CompareExchange(ref _transactionsSupportedState, 1, 0);
                }
                catch (MongoException ex) when (IsTransactionNotSupported(ex))
                {
                    Volatile.Write(ref _transactionsSupportedState, -1);

                    if (session.IsInTransaction)
                    {
                        await session.AbortTransactionAsync(cancellationToken);
                    }

                    await SaveChangesAsync(session: null, cancellationToken);
                }
            }

            await AfterSavedAsync(cancellationToken);
            return true;
        }
        catch (Exception)
        {
            if (session.IsInTransaction)
            {
                await session.AbortTransactionAsync(cancellationToken);
            }
            throw;
        }
    }

    private async Task<bool> SaveEntitiesWithExternalSessionAsync(
        [NotNull] IClientSessionHandle session,
        CancellationToken cancellationToken)
    {
        // Caller manages the session/transaction (e.g., MassTransit transactional outbox).
        await SaveChangesAsync(session, cancellationToken);
        await AfterSavedAsync(cancellationToken);
        return true;
    }

    private async Task<bool> SaveEntitiesWithoutSessionAsync(CancellationToken cancellationToken)
    {
        // Standalone Mongo: no sessions, no transactions.
        await SaveChangesAsync(session: null, cancellationToken);
        await AfterSavedAsync(cancellationToken);
        return true;
    }

    private async Task AfterSavedAsync(CancellationToken cancellationToken)
    {
        await DispatchDomainEventsBestEffortAsync(cancellationToken);
        ClearSaved();
    }

    private async Task SaveChangesAsync(IClientSessionHandle? session, CancellationToken cancellationToken)
    {
        // Snapshot to avoid enumeration invalidation if anything modifies tracking indirectly.
        var trackedEntities = _trackedEntities.Values.ToList();

        foreach (var tracked in trackedEntities)
        {
            var aggregate = tracked.Aggregate;

            var effectiveState = tracked.State;
            if (effectiveState == EntityState.Unchanged)
            {
                continue;
            }

            var writer = GetWriter(aggregate.GetType());

            switch (effectiveState)
            {
                case EntityState.New:
                    await writer.InsertAsync(session, aggregate, cancellationToken);
                    break;

                case EntityState.Dirty:
                    await writer.ReplaceAsync(session, aggregate, cancellationToken);
                    break;

                case EntityState.Deleted:
                    await writer.DeleteAsync(session, aggregate.Id, cancellationToken);
                    break;
            }
        }
    }

    private async Task DispatchDomainEventsBestEffortAsync(CancellationToken cancellationToken = default)
    {
        var entitiesWithEvents = _trackedEntities.Values
            .Select(x => (IHasDomainEvents)x.Aggregate)
            .Where(x => x.DomainEvents.Count > 0)
            .ToList();

        if (entitiesWithEvents.Count == 0)
        {
            return;
        }

        try
        {
            await _domainEventDispatcher.DispatchAndClearEvents(entitiesWithEvents);
        }
        catch (Exception ex)
        {
            // Best-effort: persistence succeeded; allow retry of dispatch.
            MongoUnitOfWorkLogger.LogDomainEventsDispatchFailed(_logger, ex);
        }
    }

    private void ClearSaved()
    {
        var keys = _trackedEntities.Keys.ToList();

        foreach (var key in keys)
        {
            var tracked = _trackedEntities[key];

            if (tracked.State == EntityState.Deleted)
            {
                // If dispatch failed, keep deleted aggregates that still have domain events
                // so we can retry dispatch without re-running the delete.
                if (tracked.Aggregate.DomainEvents.Count == 0)
                {
                    _trackedEntities.Remove(key);
                    continue;
                }

                tracked.State = EntityState.Unchanged;
                continue;
            }

            tracked.State = EntityState.Unchanged;
        }
    }

    private static string GetKey(Type aggregateType, Guid id)
        => $"{aggregateType.FullName ?? aggregateType.Name}:{id}";

    private IMongoAggregateWriter GetWriter(Type aggregateType)
    {
        if (_writers.TryGetValue(aggregateType, out var writer))
        {
            return writer;
        }

        // Allow writers registered for a base type.
        var assignableWriter = _writers
            .Where(kvp => kvp.Key.IsAssignableFrom(aggregateType))
            .Select(kvp => kvp.Value)
            .FirstOrDefault();

        return assignableWriter
            ?? throw new InvalidOperationException(
                $"No IMongoAggregateWriter registered for aggregate type '{aggregateType.FullName}'.");
    }

    private static bool IsTransactionNotSupported(MongoException ex)
    {
        // Common server message when MongoDB is not a replica set / mongos.
        var msg = ex.Message ?? string.Empty;
        return msg.Contains("Transaction numbers are only allowed", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("replica set", StringComparison.OrdinalIgnoreCase)
            || msg.Contains("mongos", StringComparison.OrdinalIgnoreCase);
    }
}

