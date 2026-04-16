using Microsoft.Extensions.Logging;
using SaaS.PowerBnB.Modules.Charging.Infrastructure.Data;
using SaaS.PowerBnB.SharedKernel.Infrastructure.Outbox;


namespace SaaS.PowerBnB.Modules.Charging.Infrastructure.Workers;

internal class ChargingOutboxWorker : OutboxProcessorJobBase<ChargingDbContext>
{
    public ChargingOutboxWorker(IServiceProvider serviceProvider, ILogger<ChargingOutboxWorker> logger)
        : base(serviceProvider, logger)
    {
    }
}