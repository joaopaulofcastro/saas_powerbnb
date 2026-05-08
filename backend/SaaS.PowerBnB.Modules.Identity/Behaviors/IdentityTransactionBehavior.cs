using MediatR;
using OneOf;
using SaaS.PowerBnB.Modules.Identity.CQRS;
using SaaS.PowerBnB.Modules.Identity.Infrastructure.Data;
using SaaS.PowerBnB.SharedKernel.Application.Errors;
using SaaS.PowerBnB.SharedKernel.Data;

namespace SaaS.PowerBnB.Modules.Identity.Behaviors;

internal class IdentityTransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IIdentityCommand<TResponse>
{
    private readonly IUnitOfWork<IdentityDbContext> _unitOfWork;

    public IdentityTransactionBehavior(IUnitOfWork<IdentityDbContext> unitOfWork)
        => _unitOfWork = unitOfWork;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        // Não persiste se houve falha de validação
        if (response is IOneOf oneOfResult && oneOfResult.Value is ValidationFailed)
            return response;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return response;
    }
}
