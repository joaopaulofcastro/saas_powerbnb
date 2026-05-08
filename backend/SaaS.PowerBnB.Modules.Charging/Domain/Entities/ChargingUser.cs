using SaaS.PowerBnB.Modules.Charging.Domain.Enums;
using SaaS.PowerBnB.SharedKernel.Domain;

namespace SaaS.PowerBnB.Modules.Charging.Domain.Entities;

/// <summary>
/// Tabela espelho de usuário no módulo Charging.
/// Criada automaticamente ao receber UserRegisteredEvent do módulo Identity.
/// Id = UserId do evento (sub do Keycloak).
/// </summary>
internal class ChargingUser : EntityBase
{
    public ChargingUserStatus Status { get; private set; }

    // Construtor para o EF Core
    private ChargingUser() { }

    public ChargingUser(Guid id)
    {
        Id = id;
        Status = ChargingUserStatus.Active;
    }
}
