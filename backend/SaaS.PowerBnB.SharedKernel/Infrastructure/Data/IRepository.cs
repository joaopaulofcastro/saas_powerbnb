using SaaS.PowerBnB.SharedKernel.Domain;

namespace SaaS.PowerBnB.SharedKernel.Infrastructure.Data;

public interface IRepository<T> where T : AggregateRoot
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void Update(T entity);

    void Delete(T entity);
}
