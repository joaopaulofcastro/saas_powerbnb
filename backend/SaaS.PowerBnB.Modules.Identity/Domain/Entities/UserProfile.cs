using SaaS.PowerBnB.Modules.Identity.Domain.Enums;
using SaaS.PowerBnB.Modules.Identity.Domain.Events;
using SaaS.PowerBnB.SharedKernel.Domain;

namespace SaaS.PowerBnB.Modules.Identity.Domain.Entities;

internal class UserProfile : AggregateRoot
{
    public string Email { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public UserStatus Status { get; private set; }

    // Construtor para o EF Core
    private UserProfile() { }

    /// <summary>
    /// Cria um novo UserProfile com o Id externo fornecido pelo Keycloak (sub do JWT).
    /// O Id NÃO é gerado pelo sistema — é o mesmo UUID do Keycloak.
    /// </summary>
    public UserProfile(Guid id, string email, string name) : base(id)
    {
        Email = email;
        Name = name;
        Status = UserStatus.Active;

        AddDomainEvent(new UserRegisteredDomainEvent(id, email, name, DateTime.UtcNow));
    }
}
