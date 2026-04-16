using NetTopologySuite.Geometries;
using SaaS.PowerBnB.Modules.Charging.Domain.Enums;
using SaaS.PowerBnB.Modules.Charging.Domain.Events;
using SaaS.PowerBnB.SharedKernel.Domain;
using SaaS.PowerBnB.SharedKernel.Domain.Constants;

namespace SaaS.PowerBnB.Modules.Charging.Domain.Entities;

internal class ChargingPoint : AggregateRoot
{
    public string Title { get; private set; } = string.Empty;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public Guid HostId { get; private set; }

    public ConnectorType Connector { get; private set; }
    public decimal MaxPowerKw { get; private set; } // Ex: 7.4 kW ou 22 kW
    public decimal PricePerKwh { get; private set; } // Preço definido pelo host

    // Máquina de Estados
    public ChargerStatus Status { get; private set; }
    public DateTimeOffset? LastPingAt { get; private set; } // Última vez que o MQTT deu sinal de vida

    public Point Location { get; private set; } = null!;

    private ChargingPoint() { }

    public ChargingPoint(string title, double lat, double lon, Guid hostId,
                         ConnectorType connector, decimal maxPowerKw, decimal pricePerKwh)
    {
        Title = title;
        Latitude = lat;
        Longitude = lon;
        HostId = hostId;
        Connector = connector;
        MaxPowerKw = maxPowerKw;
        PricePerKwh = pricePerKwh;

        // Todo carregador nasce Offline até o anfitrião plugar na tomada/Wi-Fi
        Status = ChargerStatus.Offline;
        Location = new Point(lon, lat) { SRID = SpatialConstants.Wgs84 };

        AddDomainEvent(new PointRegisteredEvent(this.Id, this.Title, this.HostId, this.Latitude, this.Longitude, DateTime.UtcNow));
    }

    public void ReceivePing()
    {
        LastPingAt = DateTime.UtcNow;

        if (Status == ChargerStatus.Offline)
        {
            Status = ChargerStatus.Available;
            // AddDomainEvent(new ChargerCameOnlineEvent(...));
        }
    }

    public void StartSession()
    {
        if (Status != ChargerStatus.Available)
            throw new InvalidOperationException("O carregador não está disponível para uso.");

        Status = ChargerStatus.Occupied;
        // AddDomainEvent(new ChargeSessionStartedEvent(...));
    }

    public void EndSession()
    {
        Status = ChargerStatus.Available;
    }

    public void PutInMaintenance()
    {
        if (Status == ChargerStatus.Occupied)
            throw new InvalidOperationException("Não é possível entrar em manutenção durante uma recarga.");

        Status = ChargerStatus.Maintenance;
    }

    public void UpdateDetails(string newTitle, double newLat, double newLon)
    {
        Title = newTitle;
        Latitude = newLat;
        Longitude = newLon;

        AddDomainEvent(new PointUpdatedEvent(
            PointId: this.Id,
            NewTitle: this.Title,
            OccurredOn: DateTime.UtcNow
        ));
    }
}