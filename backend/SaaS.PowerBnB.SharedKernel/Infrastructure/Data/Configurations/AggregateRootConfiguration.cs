using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.PowerBnB.SharedKernel.Domain;


namespace SaaS.PowerBnB.SharedKernel.Infrastructure.Data.Configurations;

public abstract class AggregateRootConfiguration<TEntity> : EntityBaseConfiguration<TEntity>
    where TEntity : AggregateRoot
{
    // Escondemos o método da classe base e travamos (sealed)
    public override sealed void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // 1. Executa a configuração da EntityBase (o Id)
        base.Configure(builder);

        // 2. A sua regra de ouro: O EF Core deve ignorar as fofocas internas
        builder.Ignore(e => e.DomainEvents);
    }
}