using MediatR;
using OneOf;
using SaaS.PowerBnB.Modules.Charging.Domain.Enums;
using SaaS.PowerBnB.Modules.Charging.Infrastructure.Repositories;
using SaaS.PowerBnB.SharedKernel.Application.Errors;

namespace SaaS.PowerBnB.Modules.Charging.Features.UpdatePoint;

internal class UpdatePointHandler : IRequestHandler<UpdatePointCommand, OneOf<Unit, ValidationFailed>>
{
    private readonly IChargingPointRepository _repository;

    public UpdatePointHandler(IChargingPointRepository repository)
    {
        _repository = repository;
    }

    public async Task<OneOf<Unit, ValidationFailed>> Handle(UpdatePointCommand request, CancellationToken cancellationToken)
    {
        // 1. Busca o ponto pelo Id
        var point = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (point is null)
            return new ValidationFailed(new Error("Point.NotFound", $"Ponto de carregamento '{request.Id}' não encontrado."));

        // 2. Verifica se o host é o dono do ponto
        if (point.HostId != request.HostId)
            return new ValidationFailed(new Error("Point.Forbidden", "Você não tem permissão para editar este ponto de carregamento."));

        // 3. Verifica se o ponto não está em uso
        if (point.Status == ChargerStatus.Occupied)
            return new ValidationFailed(new Error("Point.Occupied", "Não é possível editar um ponto com sessão de recarga ativa."));

        // 4. Aplica as alterações via método de domínio
        point.UpdateDetails(request.Title, request.Connector, request.MaxPowerKw, request.PricePerKwh);
        _repository.Update(point);

        // O ChargingTransactionBehavior faz o SaveChanges e processa o Outbox
        return Unit.Value;
    }
}
