using Ardalis.SharedKernel;
using Ardalis.Specification;

namespace RestoRate.SharedKernel.Interfaces;

public interface IRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot
{
}
