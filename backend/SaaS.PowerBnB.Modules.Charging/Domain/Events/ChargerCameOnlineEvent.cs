using MediatR;

namespace SaaS.PowerBnB.Modules.Charging.Domain.Events;

internal record ChargerCameOnlineEvent(
    Guid PointId,
    Guid HostId,
    DateTime OccurredOn
) : INotification;
