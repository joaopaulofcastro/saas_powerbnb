using Microsoft.Extensions.Logging;
using SaaS.PowerBnB.Modules.Charging.Infrastructure.Data;
using SaaS.PowerBnB.SharedKernel.Infrastructure.Outbox;
using System.Diagnostics;


namespace SaaS.PowerBnB.Modules.Charging.Infrastructure.Workers;

internal class ChargingOutboxWorker : OutboxProcessorJobBase<ChargingDbContext>
{
    private static readonly ActivitySource _activitySource = new("SaaS.PowerBnB.Modules.Charging");

    public ChargingOutboxWorker(IServiceProvider serviceProvider, ILogger<ChargingOutboxWorker> logger)
        : base(serviceProvider, logger)
    {
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = _activitySource.StartActivity("OutboxProcessor Process", ActivityKind.Internal);

        activity?.SetTag("worker.name", "ChargingOutboxWorker");
        activity?.SetTag("worker.module", "Charging");
        activity?.SetTag("outbox.db_context", nameof(ChargingDbContext));

        try
        {
            await base.ExecuteAsync(stoppingToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }
}