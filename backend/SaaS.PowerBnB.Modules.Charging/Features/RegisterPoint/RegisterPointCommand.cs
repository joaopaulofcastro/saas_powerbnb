using OneOf;
using SaaS.PowerBnB.Modules.Charging.CQRS;
using SaaS.PowerBnB.Modules.Charging.Domain.Enums;
using SaaS.PowerBnB.SharedKernel.Application.Errors;


namespace SaaS.PowerBnB.Modules.Charging.Features.RegisterPoint;

internal record RegisterPointCommand(string Title, double Lat, double Lon, Guid HostId, ConnectorType ConnectorType, decimal MaxPowerKw, decimal PricePerKwh) 
    : IChargingCommand<OneOf<Guid, ValidationFailed>>;