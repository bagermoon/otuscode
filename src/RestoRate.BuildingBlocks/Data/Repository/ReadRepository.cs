using Ardalis.SharedKernel;
using Ardalis.Specification.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;

namespace RestoRate.BuildingBlocks.Data.Repository;

public class ReadRepository<TEntity, TContext> : RepositoryBase<TEntity>, IReadRepository<TEntity>
    where TEntity : class, IAggregateRoot
    where TContext : DbContext
{
    public ReadRepository(TContext dbContext) : base(dbContext)
    {
    }
}