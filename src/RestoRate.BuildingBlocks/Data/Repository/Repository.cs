using Ardalis.Result;
using Ardalis.SharedKernel;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;

using RestoRate.SharedKernel.Filters;

namespace RestoRate.BuildingBlocks.Data.Repository;

public class Repository<TEntity, TContext> : RepositoryBase<TEntity>, IRepository<TEntity>, IReadRepository<TEntity>
    where TEntity : class, IAggregateRoot
    where TContext : DbContext
{
    public Repository(TContext dbContext) : base(dbContext)
    {
    }

    public virtual async Task<PagedResult<List<TEntity>>> ListAsync(ISpecification<TEntity> specification, BaseFilter filter, CancellationToken cancellationToken)
    {
        var count = await ApplySpecification(specification).CountAsync(cancellationToken);
        var pagination = new Pagination(count, filter);
        var info = new PagedInfo(
                pageNumber: pagination.Page,
                pageSize: pagination.Take,
                totalPages: pagination.TotalPages,
                totalRecords: pagination.TotalItems
            );

        if (count == 0)
        {
            return new PagedResult<List<TEntity>>(info, new List<TEntity>());
        }

        var data = await ApplySpecification(specification)
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .ToListAsync(cancellationToken);


        return new PagedResult<List<TEntity>>(info, data);
    }
}
