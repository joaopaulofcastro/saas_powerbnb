using Microsoft.EntityFrameworkCore;
using SaaS.PowerBnB.Modules.Charging.Domain.Entities;
using SaaS.PowerBnB.SharedKernel.Infrastructure.Data;

namespace SaaS.PowerBnB.Modules.Charging.Infrastructure.Data;

internal class ChargingDbContext : ModuleDbContextBase
{
    public ChargingDbContext(
        DbContextOptions<ChargingDbContext> options) : base(options)
    {
    }

    public DbSet<ChargingPoint> ChargingPoints => Set<ChargingPoint>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("charging");
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ChargingDbContext).Assembly);
    }
}
