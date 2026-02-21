using Ardalis.SharedKernel;

using RestoRate.Abstractions.Persistence;

using MongoDB.Driver;

using Microsoft.Extensions.Logging;

using System.Collections.ObjectModel;

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
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly ILogger<MongoUnitOfWork> _logger;
    private readonly ReadOnlyDictionary<Type, IMongoAggregateWriter> _writers;
    private readonly Dictionary<string, TrackedEntity> _trackedEntities;
    private readonly ISessionHolder _sessionHolder;

    public MongoUnitOfWork(
        IEnumerable<IMongoAggregateWriter> writers,
        ISessionHolder sessionHolder,
        IDomainEventDispatcher domainEventDispatcher,
        ILogger<MongoUnitOfWork> logger
    )
    {
        _domainEventDispatcher = domainEventDispatcher;
        _logger = logger;
        _sessionHolder = sessionHolder;
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

        /// <summary>
        /// True when a delete operation has already been persisted for this aggregate.
        /// Used to avoid re-deleting while still allowing cleanup after delayed event dispatch.
        /// </summary>
        public bool DeleteApplied { get; set; }
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

            if (state != EntityState.Deleted)
            {
                existing.DeleteApplied = false;
            }

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

    private bool HasPersistenceChanges() => _trackedEntities.Values.Any(x =>
        x.State != EntityState.Unchanged);

    private bool HasDomainEvents() => _trackedEntities.Values.Any(x =>
        ((IHasDomainEvents)x.Aggregate).DomainEvents.Count > 0);

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        if (!HasPersistenceChanges()) return true;

        var session = await _sessionHolder.GetSession(cancellationToken);
        await SaveChangesAsync(session, cancellationToken);
        ClearSaved();
        return true;
    }

    public async Task FlushDomainEventsAsync(CancellationToken cancellationToken = default)
    {
        if (!HasDomainEvents())
        {
            return;
        }

        await DispatchDomainEventsAsync(cancellationToken);
        CleanupTombstones();
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
                    tracked.DeleteApplied = true;
                    break;
            }
        }
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
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
            MongoUnitOfWorkLogger.LogDomainEventsDispatchFailed(_logger, ex);
            throw;
        }
    }

    private void CleanupTombstones()
    {
        var keys = _trackedEntities.Keys.ToList();

        foreach (var key in keys)
        {
            var tracked = _trackedEntities[key];

            if (tracked.DeleteApplied
                && tracked.Aggregate.DomainEvents.Count == 0
                && (tracked.State == EntityState.Unchanged || tracked.State == EntityState.Deleted))
            {
                _trackedEntities.Remove(key);
            }
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
                if (tracked.DeleteApplied)
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

                // Delete wasn't applied (unexpected in ClearSaved-after-save). Keep as Deleted for retry.
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

}

