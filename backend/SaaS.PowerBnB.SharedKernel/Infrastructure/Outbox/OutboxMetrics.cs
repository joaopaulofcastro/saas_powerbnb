using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace SaaS.PowerBnB.SharedKernel.Infrastructure.Outbox;

public class OutboxMetrics
{
    public static readonly string MeterName = "SaaS.PowerBnB.Metrics.Outbox";
    private static readonly Meter _meter = new(MeterName, "1.0.0");

    // Dicionário Thread-Safe para guardar a fila de MÚLTIPLOS workers
    private static readonly ConcurrentDictionary<string, int> _pendingCounts = new();

    static OutboxMetrics()
    {
        _meter.CreateObservableGauge<int>(
            name: "outbox_pending_records",
            observeValues: () =>
            {
                return _pendingCounts.Select(kvp =>
                    new Measurement<int>(
                        kvp.Value,
                        new KeyValuePair<string, object?>("module_context", kvp.Key)
                    )
                ).ToArray();
            },
            description: "Número de registros aguardando processamento no Outbox");
    }

    public static void UpdatePendingRecords(string contextName, int count)
    {
        _pendingCounts[contextName] = count;
    }
}