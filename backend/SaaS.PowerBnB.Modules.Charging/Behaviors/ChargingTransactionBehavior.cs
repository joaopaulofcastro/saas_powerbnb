using MediatR;
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
        var response = await next();

        var isFailure = response?.GetType().GetProperties().Any(p => p.Name == "Value" && p.GetValue(response) is ValidationFailed) ?? false;

        if (!isFailure)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}
