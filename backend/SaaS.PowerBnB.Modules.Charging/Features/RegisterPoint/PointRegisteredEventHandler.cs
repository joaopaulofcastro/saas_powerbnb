using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using SaaS.PowerBnB.Modules.Charging.Domain.Events;
using System.Text.Json;

namespace SaaS.PowerBnB.Modules.Charging.Features.RegisterPoint;


internal class PointRegisteredEventHandler : INotificationHandler<PointRegisteredEvent>
{
    private readonly IDistributedCache _cache;

    public PointRegisteredEventHandler(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task Handle(PointRegisteredEvent notification, CancellationToken cancellationToken)
    {
        var cacheKey = $"charging-point:{notification.PointId}";
        var data = JsonSerializer.Serialize(notification);

        await _cache.SetStringAsync(cacheKey, data, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
        }, cancellationToken);
    }
}