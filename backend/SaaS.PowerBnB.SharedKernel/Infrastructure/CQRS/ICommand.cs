using MediatR;

namespace SaaS.PowerBnB.SharedKernel.CQRS;

public interface ICommand<TResponse> : IRequest<TResponse> { }