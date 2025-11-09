using System;

using Ardalis.SharedKernel;

using Microsoft.EntityFrameworkCore;

namespace RestoRate.BuildingBlocks.Data.DbContexts;

public abstract class DbContextBase : DbContext
{
    public DbContextBase(DbContextOptions options) : base(options)
    {}

    public IEnumerable<HasDomainEventsBase> GetEntitiesWithDomainEvents()
        => ChangeTracker.Entries<HasDomainEventsBase>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToArray();
}
