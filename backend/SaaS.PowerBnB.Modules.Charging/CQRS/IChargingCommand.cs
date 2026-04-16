using SaaS.PowerBnB.SharedKernel.CQRS;

namespace SaaS.PowerBnB.Modules.Charging.CQRS;

internal interface IChargingCommand<TResponse> : ICommand<TResponse> { }
