using Microsoft.Extensions.Configuration;
using SaaS.PowerBnB.SharedKernel.CQRS;

namespace SaaS.PowerBnB.Modules.Charging.Features.GetNearbyPoints;

/// <summary>
/// Query para busca de pontos de carregamento próximos.
/// Implementa ICachedQuery para que o QueryCachingBehavior gerencie o cache Redis automaticamente.
/// A CacheKey é derivada do geohash da localização, agrupando usuários na mesma célula geográfica.
/// </summary>
internal record GetNearbyPointsQuery : ICachedQuery<IReadOnlyList<NearbyPointDto>>
{
    public double Lat { get; init; }
    public double Lon { get; init; }
    public double RadiusKm { get; init; }
    public string CacheKey { get; init; }
    public TimeSpan? Expiration { get; init; }

    public GetNearbyPointsQuery(double lat, double lon, double radiusKm, IConfiguration configuration)
    {
        Lat = lat;
        Lon = lon;
        RadiusKm = radiusKm;

        var precision = configuration.GetValue<int>("Charging:GeohashPrecision", 5);
        var geohash = GeohashHelper.Encode(lat, lon, precision);
        CacheKey = $"nearby-points:{geohash}";

        var expirationStr = configuration["Charging:NearbyPointsCacheExpiration"];
        Expiration = TimeSpan.TryParse(expirationStr, out var parsed)
            ? parsed
            : TimeSpan.FromMinutes(2);
    }
}
