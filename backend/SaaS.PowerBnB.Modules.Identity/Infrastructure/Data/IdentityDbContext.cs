using Microsoft.EntityFrameworkCore;
using SaaS.PowerBnB.Modules.Identity.Domain.Entities;
using SaaS.PowerBnB.SharedKernel.Infrastructure.Data;

namespace SaaS.PowerBnB.Modules.Identity.Infrastructure.Data;

internal class IdentityDbContext : ModuleDbContextBase
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options) { }

    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    }
}
