using MediatR;

namespace SaaS.PowerBnB.Modules.Charging.Domain.Events;

internal record PointRegisteredEvent(
    Guid PointId,
    string Title,
    Guid HostId,
    double Latitude,
    double Longitude,
    DateTime OccurredOn
) : INotification;
