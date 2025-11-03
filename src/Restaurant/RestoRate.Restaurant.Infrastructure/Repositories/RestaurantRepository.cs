using Ardalis.SharedKernel;
using Ardalis.Specification;
using RestoRate.Restaurant.Infrastructure.Data;

namespace RestoRate.Restaurant.Infrastructure.Repositories;

public class RestaurantRepository(RestaurantDbContext context) : IRepository<Domain.RestaurantAggregate.Restaurant>
{
    private IRepository<Domain.RestaurantAggregate.Restaurant> _repositoryImplementation;


    public Task<Domain.RestaurantAggregate.Restaurant?> GetByIdAsync<TId>(TId id, CancellationToken cancellationToken = new CancellationToken()) where TId : notnull => throw new NotImplementedException();

    public Task<Domain.RestaurantAggregate.Restaurant?> FirstOrDefaultAsync(ISpecification<Domain.RestaurantAggregate.Restaurant> specification, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<Domain.RestaurantAggregate.Restaurant, TResult> specification,
        CancellationToken cancellationToken = new CancellationToken()) =>
        throw new NotImplementedException();

    public Task<Domain.RestaurantAggregate.Restaurant?> SingleOrDefaultAsync(ISingleResultSpecification<Domain.RestaurantAggregate.Restaurant> specification,
        CancellationToken cancellationToken = new CancellationToken()) =>
        throw new NotImplementedException();

    public Task<TResult?> SingleOrDefaultAsync<TResult>(ISingleResultSpecification<Domain.RestaurantAggregate.Restaurant, TResult> specification,
        CancellationToken cancellationToken = new CancellationToken()) =>
        throw new NotImplementedException();

    public Task<List<Domain.RestaurantAggregate.Restaurant>> ListAsync(CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public Task<List<Domain.RestaurantAggregate.Restaurant>> ListAsync(ISpecification<Domain.RestaurantAggregate.Restaurant> specification, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public Task<List<TResult>> ListAsync<TResult>(ISpecification<Domain.RestaurantAggregate.Restaurant, TResult> specification, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public Task<int> CountAsync(ISpecification<Domain.RestaurantAggregate.Restaurant> specification, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public Task<int> CountAsync(CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public Task<bool> AnyAsync(ISpecification<Domain.RestaurantAggregate.Restaurant> specification, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public Task<bool> AnyAsync(CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public IAsyncEnumerable<Domain.RestaurantAggregate.Restaurant> AsAsyncEnumerable(ISpecification<Domain.RestaurantAggregate.Restaurant> specification) => throw new NotImplementedException();

    public async Task<Domain.RestaurantAggregate.Restaurant> AddAsync(Domain.RestaurantAggregate.Restaurant restaurant, CancellationToken cancellationToken = default)
    {
        await context.Restaurants.AddAsync(restaurant, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return restaurant;
    }

    public Task<IEnumerable<Domain.RestaurantAggregate.Restaurant>> AddRangeAsync(IEnumerable<Domain.RestaurantAggregate.Restaurant> entities, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public async Task<int> UpdateAsync(
        Domain.RestaurantAggregate.Restaurant restaurant,
        CancellationToken cancellationToken = default)
    {
        context.Restaurants.Update(restaurant);
        return await context.SaveChangesAsync(cancellationToken);
    }

    public Task<int> UpdateRangeAsync(IEnumerable<Domain.RestaurantAggregate.Restaurant> entities, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public async Task<int> DeleteAsync(
        Domain.RestaurantAggregate.Restaurant restaurant,
        CancellationToken cancellationToken = default)
    {
        context.Restaurants.Remove(restaurant);
        return await context.SaveChangesAsync(cancellationToken);
    }

    public Task<int> DeleteRangeAsync(IEnumerable<Domain.RestaurantAggregate.Restaurant> entities, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public Task<int> DeleteRangeAsync(ISpecification<Domain.RestaurantAggregate.Restaurant> specification, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();
}
