using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SaaS.PowerBnB.Modules.Charging.Domain.Events;


namespace SaaS.PowerBnB.Modules.Charging.Features.Connectivity;

internal class ChargerCameOnlineEventHandler : INotificationHandler<ChargerCameOnlineEvent>
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<ChargerCameOnlineEventHandler> _logger;
    // private readonly IPushNotificationService _pushService;

    public ChargerCameOnlineEventHandler(
        IDistributedCache cache,
        ILogger<ChargerCameOnlineEventHandler> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task Handle(ChargerCameOnlineEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Carregador {PointId} voltou a ficar online. Processando efeitos colaterais...", notification.PointId);

        // 1. Invalida o cache da região para forçar o mapa a ler do PostGIS na próxima busca
        var cacheKey = $"points_status:{notification.PointId}";
        await _cache.RemoveAsync(cacheKey, cancellationToken);

        // 2. Envia Push Notification para o celular do Anfitrião
        // await _pushService.SendToUserAsync(
        //     userId: notification.HostId, 
        //     title: "Equipamento Online \u2705", 
        //     body: "Seu carregador voltou a se comunicar com a nossa rede e já está visível no mapa!"
        // );
    }
}