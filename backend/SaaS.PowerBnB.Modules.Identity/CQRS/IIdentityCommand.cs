using SaaS.PowerBnB.SharedKernel.CQRS;

namespace SaaS.PowerBnB.Modules.Identity.CQRS;

internal interface IIdentityCommand<TResponse> : ICommand<TResponse> { }
