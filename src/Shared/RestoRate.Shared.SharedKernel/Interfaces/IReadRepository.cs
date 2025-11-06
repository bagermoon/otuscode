using Ardalis.Specification;

namespace RestoRate.Shared.SharedKernel.Interfaces;

public interface IReadRepository<T> : IReadRepositoryBase<T> where T : class
{
}
