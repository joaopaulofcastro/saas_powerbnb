using Microsoft.EntityFrameworkCore;
using SaaS.PowerBnB.SharedKernel.Audit;
using SaaS.PowerBnB.SharedKernel.Infrastructure.Outbox;


namespace SaaS.PowerBnB.SharedKernel.Infrastructure.Data;

public abstract class ModuleDbContextBase : DbContext
{
    protected ModuleDbContextBase(DbContextOptions options) : base(options)
    {
    }

    // Tabelas universais que todo módulo terá
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração Padrão do Outbox (Sem usar arquivos externos para ser direto na base)
        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Type).IsRequired();
            builder.Property(x => x.Content).IsRequired();
            builder.Property(x => x.Error);
        });

        // Configuração Padrão do AuditLog
        modelBuilder.Entity<AuditLog>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.TableName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Action).IsRequired().HasMaxLength(50);
            builder.Property(x => x.OldValues);
            builder.Property(x => x.NewValues);
        });
    }
}