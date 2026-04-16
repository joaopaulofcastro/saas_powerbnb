using SaaS.PowerBnB.Modules.Charging.Domain.Entities;
using SaaS.PowerBnB.SharedKernel.Infrastructure.Data;

namespace SaaS.PowerBnB.Modules.Charging.Infrastructure.Repositories;

internal interface IChargingPointRepository : IRepository<ChargingPoint> 
{
    Task<bool> ExistsTitleForHostAsync(Guid hostId, string title, CancellationToken cancellationToken = default);
}