using MediatR;
using SaaS.PowerBnB.SharedKernel.Infrastructure.Data;

namespace SaaS.PowerBnB.Modules.Charging.Features.GetNearbyPoints;

internal class GetNearbyPointsHandler : IRequestHandler<GetNearbyPointsQuery, IReadOnlyList<NearbyPointDto>>
{
    private readonly SqlExecutor _sqlExecutor;

    // SQL com ST_DWithin para filtrar por raio e ST_Distance para calcular a distância
    // Filtra apenas pontos com status = 1 (Available)
    private const string NearbyPointsSql = """
        SELECT
            id                                                              AS "Id",
            title                                                           AS "Title",
            latitude                                                        AS "Latitude",
            longitude                                                       AS "Longitude",
            connector                                                       AS "Connector",
            max_power_kw                                                    AS "MaxPowerKw",
            price_per_kwh                                                   AS "PricePerKwh",
            ST_Distance(
                location::geography,
                ST_SetSRID(ST_MakePoint(@Lon, @Lat), 4326)::geography
            ) / 1000.0                                                      AS "DistanceKm"
        FROM charging.charging_points
        WHERE
            status = 1
            AND ST_DWithin(
                location::geography,
                ST_SetSRID(ST_MakePoint(@Lon, @Lat), 4326)::geography,
                @RadiusMeters
            )
        ORDER BY "DistanceKm";
        """;

    public GetNearbyPointsHandler(SqlExecutor sqlExecutor)
    {
        _sqlExecutor = sqlExecutor;
    }

    public async Task<IReadOnlyList<NearbyPointDto>> Handle(
        GetNearbyPointsQuery request,
        CancellationToken cancellationToken)
    {
        var results = await _sqlExecutor.QueryAsync<NearbyPointDto>(
            NearbyPointsSql,
            new
            {
                request.Lat,
                request.Lon,
                RadiusMeters = request.RadiusKm * 1000.0
            },
            cancellationToken);

        return results.ToList().AsReadOnly();
    }
}
