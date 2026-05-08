using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using SaaS.PowerBnB.Modules.Charging.Domain.Events;
using SaaS.PowerBnB.Modules.Charging.Features.GetNearbyPoints;

namespace SaaS.PowerBnB.Modules.Charging.Features.UpdatePoint;

internal class PointUpdatedEventHandler : INotificationHandler<PointUpdatedEvent>
{
    private readonly IDistributedCache _cache;
    private readonly IConfiguration _configuration;

    public PointUpdatedEventHandler(IDistributedCache cache, IConfiguration configuration)
    {
        _cache = cache;
        _configuration = configuration;
    }

    public async Task Handle(PointUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var precision = _configuration.GetValue<int>("Charging:GeohashPrecision", 5);
        var geohash = GeohashHelper.Encode(notification.Latitude, notification.Longitude, precision);
        var cacheKey = $"nearby-points:{geohash}";

        await _cache.RemoveAsync(cacheKey, cancellationToken);
    }
}
