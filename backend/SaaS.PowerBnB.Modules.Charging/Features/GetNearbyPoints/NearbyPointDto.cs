namespace SaaS.PowerBnB.Modules.Charging.Features.GetNearbyPoints;

/// <summary>
/// DTO de leitura retornado pelo GetNearbyPointsHandler.
/// Projetado diretamente da query Dapper com ST_DWithin + ST_Distance.
/// </summary>
internal record NearbyPointDto(
    Guid Id,
    string Title,
    double Latitude,
    double Longitude,
    int Connector,
    decimal MaxPowerKw,
    decimal PricePerKwh,
    double DistanceKm
);
