using MediatR;
using OneOf;
using SaaS.PowerBnB.Modules.Charging.CQRS;
using SaaS.PowerBnB.SharedKernel.Application.Errors;

namespace SaaS.PowerBnB.Modules.Charging.Features.DeletePoint;

internal record DeletePointCommand(
    Guid Id,
    Guid HostId
) : IChargingCommand<OneOf<Unit, ValidationFailed>>;
