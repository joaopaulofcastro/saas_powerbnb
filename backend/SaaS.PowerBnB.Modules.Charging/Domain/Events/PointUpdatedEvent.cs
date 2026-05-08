using MediatR;
using SaaS.PowerBnB.Modules.Charging.Domain.Enums;

namespace SaaS.PowerBnB.Modules.Charging.Domain.Events;

internal record PointUpdatedEvent(
    Guid PointId,
    string NewTitle,
    ConnectorType NewConnector,
    decimal NewMaxPowerKw,
    decimal NewPricePerKwh,
    double Latitude,
    double Longitude,
    DateTime OccurredOn
) : INotification;
