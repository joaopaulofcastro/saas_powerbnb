using MediatR;

namespace SaaS.PowerBnB.Modules.Charging.Domain.Events;

internal record PointUpdatedEvent(
    Guid PointId,
    string NewTitle,
    DateTime OccurredOn
) : INotification;
