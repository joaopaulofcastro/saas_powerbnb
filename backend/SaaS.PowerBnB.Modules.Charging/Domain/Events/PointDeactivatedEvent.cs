using MediatR;

namespace SaaS.PowerBnB.Modules.Charging.Domain.Events;

internal record PointDeactivatedEvent(
    Guid PointId,
    Guid HostId,
    double Latitude,
    double Longitude,
    DateTime OccurredOn
) : INotification;
