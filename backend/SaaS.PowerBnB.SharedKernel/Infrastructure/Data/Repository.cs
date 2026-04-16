using Microsoft.EntityFrameworkCore;
using SaaS.PowerBnB.SharedKernel.Domain;

namespace SaaS.PowerBnB.SharedKernel.Infrastructure.Data;

public class Repository<TContext, TEntity> : IRepository<TEntity>
    where TContext : DbContext
    where TEntity : AggregateRoot
{
    protected readonly DbContext DbContext;
    protected readonly DbSet<TEntity> DbSet;

    public Repository(DbContext dbContext)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        DbSet = DbContext.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(id, cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Delete(TEntity entity)
    {
        DbSet.Remove(entity);
    }
}
