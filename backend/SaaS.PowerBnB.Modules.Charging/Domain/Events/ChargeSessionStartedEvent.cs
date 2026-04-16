using MediatR;

namespace SaaS.PowerBnB.Modules.Charging.Domain.Events;

internal record ChargeSessionStartedEvent(
    Guid PointId,
    Guid HostId,
    Guid DriverId, // Precisamos saber quem é o motorista
    DateTime OccurredOn
) : INotification;
