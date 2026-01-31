
using Ardalis.SharedKernel;

using RestoRate.BuildingBlocks.Data.Repository;
using RestoRate.ReviewService.Infrastructure.Data;

namespace RestoRate.ReviewService.Infrastructure.Repositories;

public class EfRepository<T>(ReviewDbContext context)
    : Repository<T, ReviewDbContext>(context)
    where T : class, IAggregateRoot
{
}
