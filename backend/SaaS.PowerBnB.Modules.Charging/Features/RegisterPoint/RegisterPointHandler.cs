using MediatR;
using OneOf;
using SaaS.PowerBnB.Modules.Charging.Domain.Entities;
using SaaS.PowerBnB.Modules.Charging.Infrastructure.Repositories;
using SaaS.PowerBnB.SharedKernel.Application.Errors;

namespace SaaS.PowerBnB.Modules.Charging.Features.RegisterPoint;


internal class RegisterPointHandler : IRequestHandler<RegisterPointCommand, OneOf<Guid, ValidationFailed>>
{
    private readonly IChargingPointRepository _repository;

    public RegisterPointHandler(IChargingPointRepository repository)
    {
        _repository = repository;
    }

    public async Task<OneOf<Guid, ValidationFailed>> Handle(RegisterPointCommand request, CancellationToken cancellationToken)
    {
        // 1. Validação de Negócio (Semântica)
        bool titleAlreadyExists = await _repository.ExistsTitleForHostAsync(
            request.HostId,
            request.Title,
            cancellationToken);

        if (titleAlreadyExists)
        {
            // Retorna a falha de validação via OneOf, encerrando o fluxo.
            // Adapte a inicialização do ValidationFailed conforme a sua estrutura atual.
            return new ValidationFailed(new[]
            {
                new Error("Title.Duplicated", $"Você já possui um ponto de recarga com o nome '{request.Title}'.")
            });
        }

        // 2. Criação do Agregado 
        // (Ao dar o 'new', os Eventos de Domínio já são acoplados internamente)
        var point = new ChargingPoint(
            request.Title,
            request.Lat,
            request.Lon,
            request.HostId,
            request.ConnectorType,
            request.MaxPowerKw,
            request.PricePerKwh);

        // 3. Persistência
        await _repository.AddAsync(point);

        // ⚠️ ATENÇÃO: Nós NÃO chamamos o _dbContext.SaveChangesAsync() aqui!
        // O `ChargingTransactionBehavior` que configuramos no pipeline do MediatR 
        // vai capturar essa requisição, fazer o Commit e processar o Outbox automaticamente!

        return point.Id;
    }
}