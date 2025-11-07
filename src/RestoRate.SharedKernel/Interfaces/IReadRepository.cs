using Ardalis.Specification;

namespace RestoRate.SharedKernel.Interfaces;

public interface IReadRepository<T> : IReadRepositoryBase<T> where T : class
{
}
