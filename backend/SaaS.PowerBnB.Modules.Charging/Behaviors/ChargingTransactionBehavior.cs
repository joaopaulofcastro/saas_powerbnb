using MediatR;
using OneOf;
using SaaS.PowerBnB.Modules.Charging.CQRS;
using SaaS.PowerBnB.Modules.Charging.Infrastructure.Data;
using SaaS.PowerBnB.SharedKernel.Application.Errors;
using SaaS.PowerBnB.SharedKernel.Data;

namespace SaaS.PowerBnB.Modules.Charging.Behaviors;

internal class ChargingTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IChargingCommand<TResponse>
{
    private readonly IUnitOfWork<ChargingDbContext> _unitOfWork;

    public ChargingTransactionBehavior(IUnitOfWork<ChargingDbContext> unitOfWork)
        => _unitOfWork = unitOfWork;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // 1. Executa o Handler
        var response = await next();

        // 2. Pattern Matching de alta performance (Zero Reflection)
        if (response is IOneOf oneOfResult && oneOfResult.Value is ValidationFailed)
        {
            // Ocorreu uma falha de domínio/validação. 
            // Retorna imediatamente sem chamar o SaveChangesAsync.
            return response;
        }

        // 3. Sucesso absoluto: persiste no Postgres
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}