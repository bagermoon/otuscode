using Ardalis.SharedKernel;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;

using RestoRate.Restaurant.Infrastructure.Data;

namespace RestoRate.Restaurant.Infrastructure.Repositories;

public class RestaurantReadRepository(RestaurantDbContext context)
    : RepositoryBase<Domain.RestaurantAggregate.Restaurant>(context), IReadRepository<Domain.RestaurantAggregate.Restaurant>
{
    private IReadRepository<Domain.RestaurantAggregate.Restaurant> _readRepositoryImplementation;
    public Task<Domain.RestaurantAggregate.Restaurant?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = new CancellationToken()) where TId : notnull => _readRepositoryImplementation.GetByIdAsync(id, cancellationToken);

    public Task<Domain.RestaurantAggregate.Restaurant?> FirstOrDefaultAsync(ISpecification<Domain.RestaurantAggregate.Restaurant> specification, CancellationToken cancellationToken = new CancellationToken()) => _readRepositoryImplementation.FirstOrDefaultAsync(specification, cancellationToken);

    public Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<Domain.RestaurantAggregate.Restaurant, TResult> specification,
        CancellationToken cancellationToken = new CancellationToken()) =>
        _readRepositoryImplementation.FirstOrDefaultAsync(specification, cancellationToken);

    public Task<Domain.RestaurantAggregate.Restaurant?> SingleOrDefaultAsync(ISingleResultSpecification<Domain.RestaurantAggregate.Restaurant> specification,
        CancellationToken cancellationToken = new CancellationToken()) =>
        _readRepositoryImplementation.SingleOrDefaultAsync(specification, cancellationToken);

    public Task<TResult?> SingleOrDefaultAsync<TResult>(ISingleResultSpecification<Domain.RestaurantAggregate.Restaurant, TResult> specification,
        CancellationToken cancellationToken = new CancellationToken()) =>
        _readRepositoryImplementation.SingleOrDefaultAsync(specification, cancellationToken);

    public Task<List<Domain.RestaurantAggregate.Restaurant>> ListAsync(CancellationToken cancellationToken = new CancellationToken()) => _readRepositoryImplementation.ListAsync(cancellationToken);

    public Task<List<Domain.RestaurantAggregate.Restaurant>> ListAsync(ISpecification<Domain.RestaurantAggregate.Restaurant> specification, CancellationToken cancellationToken = new CancellationToken()) => _readRepositoryImplementation.ListAsync(specification, cancellationToken);

    public Task<List<TResult>> ListAsync<TResult>(ISpecification<Domain.RestaurantAggregate.Restaurant, TResult> specification, CancellationToken cancellationToken = new CancellationToken()) => _readRepositoryImplementation.ListAsync(specification, cancellationToken);

    public Task<int> CountAsync(ISpecification<Domain.RestaurantAggregate.Restaurant> specification, CancellationToken cancellationToken = new CancellationToken()) => _readRepositoryImplementation.CountAsync(specification, cancellationToken);

    public Task<bool> AnyAsync(ISpecification<Domain.RestaurantAggregate.Restaurant> specification, CancellationToken cancellationToken = new CancellationToken()) => _readRepositoryImplementation.AnyAsync(specification, cancellationToken);

    public IAsyncEnumerable<Domain.RestaurantAggregate.Restaurant> AsAsyncEnumerable(ISpecification<Domain.RestaurantAggregate.Restaurant> specification) => _readRepositoryImplementation.AsAsyncEnumerable(specification);
}
