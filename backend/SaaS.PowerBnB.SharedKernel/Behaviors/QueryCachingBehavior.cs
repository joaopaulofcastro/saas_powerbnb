using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SaaS.PowerBnB.SharedKernel.CQRS;
using System.Text.Json;

namespace SaaS.PowerBnB.SharedKernel.Behaviors;

public class QueryCachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICachedQuery<TResponse>
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<QueryCachingBehavior<TRequest, TResponse>> _logger;

    public QueryCachingBehavior(IDistributedCache cache, ILogger<QueryCachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var queryName = typeof(TRequest).Name;
        
        // 1. Tenta buscar no Cache (Redis ou Memory)
        var cachedResponse = await _cache.GetStringAsync(request.CacheKey, cancellationToken);

        if (cachedResponse is not null)
        {
            _logger.LogInformation("Cache HIT para {QueryName} [Key: {CacheKey}]", queryName, request.CacheKey);
            return JsonSerializer.Deserialize<TResponse>(cachedResponse)!;
        }

        // 2. Cache MISS: Executa o Handler (vai no PostgreSQL via Dapper)
        _logger.LogInformation("Cache MISS para {QueryName}. Buscando no banco...", queryName);
        var response = await next();

        // 3. Salva o resultado no Cache para a próxima vez
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = request.Expiration ?? TimeSpan.FromMinutes(5)
        };

        var serializedResponse = JsonSerializer.Serialize(response);
        await _cache.SetStringAsync(request.CacheKey, serializedResponse, cacheOptions, cancellationToken);

        return response;
    }
}