using Microsoft.EntityFrameworkCore;
using SaaS.PowerBnB.Modules.Financial.Domain.Entities;
using SaaS.PowerBnB.SharedKernel.Infrastructure.Data;

namespace SaaS.PowerBnB.Modules.Financial.Infrastructure.Data;

internal class FinancialDbContext : ModuleDbContextBase
{
    public FinancialDbContext(DbContextOptions<FinancialDbContext> options) : base(options) { }

    public DbSet<FinancialUser> FinancialUsers => Set<FinancialUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("financial");
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinancialDbContext).Assembly);
    }
}
