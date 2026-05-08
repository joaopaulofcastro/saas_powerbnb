using NGeoHash;

namespace SaaS.PowerBnB.Modules.Charging.Features.GetNearbyPoints;

/// <summary>
/// Encapsula o pacote NGeoHash para cálculo de geohash.
/// Usado como chave de cache Redis para agrupar usuários próximos na mesma célula geográfica.
/// </summary>
internal static class GeohashHelper
{
    /// <summary>
    /// Codifica coordenadas geográficas em um geohash com a precisão especificada.
    /// Precisão 5 cobre ~5 km × 5 km; precisão 6 cobre ~1.2 km × 0.6 km.
    /// </summary>
    public static string Encode(double lat, double lon, int precision)
        => GeoHash.Encode(lat, lon, precision);
}
