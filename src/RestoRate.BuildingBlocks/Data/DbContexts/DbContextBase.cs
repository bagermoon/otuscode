using Ardalis.SharedKernel;

using MassTransit.EntityFrameworkCoreIntegration;

using Microsoft.EntityFrameworkCore;

namespace RestoRate.BuildingBlocks.Data.DbContexts;

public abstract class DbContextBase : SagaDbContext
{
    public DbContextBase(DbContextOptions options) : base(options)
    { }

    public IEnumerable<HasDomainEventsBase> GetEntitiesWithDomainEvents()
        => ChangeTracker.Entries<HasDomainEventsBase>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Count > 0)
            .ToArray();

    protected override IEnumerable<ISagaClassMap> Configurations => Array.Empty<ISagaClassMap>();
}
