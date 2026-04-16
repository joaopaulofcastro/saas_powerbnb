using Microsoft.EntityFrameworkCore;

namespace SaaS.PowerBnB.SharedKernel.Data;

public interface IUnitOfWork<TContext> where TContext : DbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}