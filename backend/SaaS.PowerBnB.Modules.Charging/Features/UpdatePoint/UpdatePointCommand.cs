using MediatR;
using OneOf;
using SaaS.PowerBnB.Modules.Charging.CQRS;
using SaaS.PowerBnB.Modules.Charging.Domain.Enums;
using SaaS.PowerBnB.SharedKernel.Application.Errors;

namespace SaaS.PowerBnB.Modules.Charging.Features.UpdatePoint;

internal record UpdatePointCommand(
    Guid Id,
    string Title,
    ConnectorType Connector,
    decimal MaxPowerKw,
    decimal PricePerKwh,
    Guid HostId
) : IChargingCommand<OneOf<Unit, ValidationFailed>>;
