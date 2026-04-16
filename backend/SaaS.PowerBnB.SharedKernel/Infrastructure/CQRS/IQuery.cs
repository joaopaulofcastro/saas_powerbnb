using MediatR;

namespace SaaS.PowerBnB.SharedKernel.CQRS;

public interface IQuery<TResponse> : IRequest<TResponse> { }