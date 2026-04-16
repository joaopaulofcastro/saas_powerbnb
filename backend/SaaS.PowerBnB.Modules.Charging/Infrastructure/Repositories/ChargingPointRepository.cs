using Microsoft.EntityFrameworkCore;
using SaaS.PowerBnB.Modules.Charging.Domain.Entities;
using SaaS.PowerBnB.Modules.Charging.Infrastructure.Data;
using SaaS.PowerBnB.SharedKernel.Infrastructure.Data;

namespace SaaS.PowerBnB.Modules.Charging.Infrastructure.Repositories;

internal class ChargingPointRepository : Repository<ChargingDbContext, ChargingPoint>, IChargingPointRepository
{
    public ChargingPointRepository(ChargingDbContext dbContext) : base(dbContext) { }

    public async Task<bool> ExistsTitleForHostAsync(Guid hostId, string title, CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            p => p.HostId == hostId && p.Title.ToLower() == title.ToLower(),
            cancellationToken);
    }
}
