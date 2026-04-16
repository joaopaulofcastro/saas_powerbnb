using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SaaS.PowerBnB.SharedKernel.Application.Interfaces;
using SaaS.PowerBnB.SharedKernel.Domain;


namespace SaaS.PowerBnB.SharedKernel.Infrastructure.Interceptors;


public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditableEntityInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        // 1. Pega o ID do usuário (Se for o Worker do Outbox rodando em background, será "System")
        var userId = _currentUserService.UserId ?? "System";
        var now = DateTimeOffset.UtcNow;

        // 2. Varre todas as entidades da nossa classe base que sofreram alterações
        var entries = context.ChangeTracker.Entries<EntityBase>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                // Manipulação direta pelo ChangeTracker (bypassa o 'protected set')
                entry.Property(e => e.CreatedAt).CurrentValue = now;
                entry.Property(e => e.CreatedBy).CurrentValue = userId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(e => e.UpdatedAt).CurrentValue = now;
                entry.Property(e => e.UpdatedBy).CurrentValue = userId;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}