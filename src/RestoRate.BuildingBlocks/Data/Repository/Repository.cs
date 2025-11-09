using Ardalis.SharedKernel;
using Ardalis.Specification.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;

namespace RestoRate.BuildingBlocks.Data.Repository;

public class Repository<TEntity, TContext> : RepositoryBase<TEntity>, IRepository<TEntity>
    where TEntity : class, IAggregateRoot
    where TContext : DbContext
{
    public Repository(TContext dbContext) : base(dbContext)
    {
    }
}
