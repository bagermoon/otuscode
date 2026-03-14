using Ardalis.SharedKernel;

using MongoDB.Driver;

namespace RestoRate.RatingService.Infrastructure.Data;

public abstract class BaseRepository<TAggregate> where TAggregate : EntityBase<Guid>, IAggregateRoot, new()
{
    protected readonly IMongoContext _context;
    protected readonly ISessionHolder _sessionHolder;
    protected IMongoCollection<TAggregate> Collection { get; }

    protected BaseRepository(
        IMongoContext context,
        ISessionHolder sessionHolder)
    {
        _context = context;
        _sessionHolder = sessionHolder;
        Collection = context.Collection<TAggregate>();
    }

    public async Task<TAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_context.TryGet<TAggregate>(id, out var cached))
        {
            return cached;
        }

        // Если нет в кэше, загружаем из БД
        var aggregate = await Collection
            .Find(x => x.Id.Equals(id))
            .FirstOrDefaultAsync(cancellationToken);

        if (aggregate != null)
        {
            _context.Attach(aggregate);
        }

        return aggregate;
    }

    public async Task<IReadOnlyList<TAggregate>> GetByIdsAsync(
        IEnumerable<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        var requestedIds = ids as IList<Guid> ?? ids.ToList();
        var aggregatesById = new Dictionary<Guid, TAggregate>();
        var idsToLoad = new HashSet<Guid>();

        foreach (var id in requestedIds)
        {
            if (_context.TryGet<TAggregate>(id, out var cached))
            {
                aggregatesById[id] = cached;
            }
            else
            {
                idsToLoad.Add(id);
            }
        }

        if (idsToLoad.Count > 0)
        {
            var filter = Builders<TAggregate>.Filter.In(x => x.Id, idsToLoad);
            var loaded = await Collection.Find(filter).ToListAsync(cancellationToken);

            foreach (var aggregate in loaded)
            {
                _context.Attach(aggregate);
                aggregatesById[aggregate.Id] = aggregate;
            }
        }

        var result = new List<TAggregate>(requestedIds.Count);
        foreach (var id in requestedIds)
        {
            if (aggregatesById.TryGetValue(id, out var aggregate))
            {
                result.Add(aggregate);
            }
        }

        return result;
    }

    // Добавление в коллекцию изменений (не в БД!)
    public void Add(TAggregate aggregate)
    {
        _context.MarkNew(aggregate); // Помечаем как новый
    }

    // Обновление в коллекции изменений
    public void Update(TAggregate aggregate)
    {
        _context.MarkDirty(aggregate); // Помечаем как измененный
    }

    // Удаление
    public void Remove(Guid id)
    {
        if (_context.TryGet<TAggregate>(id, out var aggregate))
        {
            _context.MarkDeleted(aggregate); // Помечаем как удаленный
        }
        else
        {
            // Если нет в кэше, можно создать "заглушку" для удаления
            var stub = new TAggregate { Id = id };
            _context.MarkDeleted(stub);
        }
    }

    protected async Task<bool> InsertDirectAsync(TAggregate aggregate, CancellationToken cancellationToken)
    {
        try
        {
            var session = await _sessionHolder.GetSessionAsync(cancellationToken);
            if (session is null)
            {
                await Collection.InsertOneAsync(aggregate, cancellationToken: cancellationToken);
            }
            else
            {
                await Collection.InsertOneAsync(session, aggregate, cancellationToken: cancellationToken);
            }

            return true;
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            return false;
        }
    }

    protected async Task<bool> UpdateDirectAsync(
        FilterDefinition<TAggregate> filter,
        UpdateDefinition<TAggregate> update,
        CancellationToken cancellationToken)
    {
        var session = await _sessionHolder.GetSessionAsync(cancellationToken);
        var result = session is null
            ? await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken)
            : await Collection.UpdateOneAsync(session, filter, update, cancellationToken: cancellationToken);

        return result.ModifiedCount > 0;
    }

    protected void TrackPersisted(TAggregate aggregate)
    {
        _context.Attach(aggregate, EntityState.Unchanged);
    }
}
