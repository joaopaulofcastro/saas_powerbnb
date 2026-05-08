using Microsoft.Extensions.Logging;
using SaaS.PowerBnB.Modules.Identity.Infrastructure.Data;
using SaaS.PowerBnB.SharedKernel.Infrastructure.Outbox;
using System.Diagnostics;

namespace SaaS.PowerBnB.Modules.Identity.Infrastructure.Workers;

internal class IdentityOutboxWorker : OutboxProcessorJobBase<IdentityDbContext>
{
    public static readonly string ActivitySourceName = "SaaS.PowerBnB.Modules.Identity";
    private static readonly ActivitySource _activitySource = new(ActivitySourceName);

    public IdentityOutboxWorker(
        IServiceProvider serviceProvider,
        ILogger<IdentityOutboxWorker> logger)
        : base(serviceProvider, logger) { }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var activity = _activitySource.StartActivity("OutboxProcessor Process", ActivityKind.Internal);
        activity?.SetTag("worker.name", "IdentityOutboxWorker");
        activity?.SetTag("worker.module", "Identity");
        activity?.SetTag("outbox.db_context", nameof(IdentityDbContext));

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
