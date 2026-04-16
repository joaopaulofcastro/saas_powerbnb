using Microsoft.EntityFrameworkCore;

namespace SaaS.PowerBnB.SharedKernel.Data;

public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : DbContext
{
    private readonly TContext _dbContext;

    public UnitOfWork(TContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Aqui o DbContext vai processar a Auditoria (AuditLog)
        // e salvar os DomainEvents na tabela OutboxMessage em uma única transação de banco.
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}