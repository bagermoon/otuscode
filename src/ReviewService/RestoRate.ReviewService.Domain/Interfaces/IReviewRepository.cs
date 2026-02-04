using Ardalis.Result;
using Ardalis.SharedKernel;
using Ardalis.Specification;

using RestoRate.ReviewService.Domain.ReviewAggregate;
using RestoRate.SharedKernel.Filters;

namespace RestoRate.ReviewService.Domain.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<PagedResult<List<Review>>> ListAsync(ISpecification<Review> specification, BaseFilter filter, CancellationToken cancellationToken = default);
}
