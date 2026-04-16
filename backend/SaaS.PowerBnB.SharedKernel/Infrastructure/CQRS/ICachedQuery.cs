using MediatR;

namespace SaaS.PowerBnB.SharedKernel.CQRS;

public interface ICachedQuery
{
    string CacheKey { get; }
    TimeSpan? Expiration { get; } // Tempo de vida do cache
}

// Interface de união para o MediatR
public interface ICachedQuery<TResponse> : IRequest<TResponse>, ICachedQuery { }