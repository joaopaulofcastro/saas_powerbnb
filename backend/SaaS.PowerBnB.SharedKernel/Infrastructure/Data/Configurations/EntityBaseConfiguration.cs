using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaS.PowerBnB.SharedKernel.Domain;


namespace SaaS.PowerBnB.SharedKernel.Infrastructure.Data.Configurations;

public abstract class EntityBaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : EntityBase
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        // 1. Configura a Chave Primária (A sua ideia principal)
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CreatedAt).IsRequired();
        builder.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);

        builder.Property(e => e.UpdatedAt);
        builder.Property(e => e.UpdatedBy).HasMaxLength(100);

        // 2. Chama o gancho para a classe filha colocar as regras específicas
        ConfigureEntity(builder);
    }

    // O método obrigatório que as classes filhas terão que implementar
    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}