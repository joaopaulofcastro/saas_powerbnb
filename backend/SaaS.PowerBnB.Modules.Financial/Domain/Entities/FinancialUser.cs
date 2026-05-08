using SaaS.PowerBnB.SharedKernel.Domain;

namespace SaaS.PowerBnB.Modules.Financial.Domain.Entities;

internal class FinancialUser : EntityBase
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? CpfCnpj { get; private set; }
    public string? Address { get; private set; }

    // Construtor para o EF Core
    private FinancialUser() { }

    /// <summary>
    /// Cria um FinancialUser com o Id externo (sub do Keycloak via UserRegisteredEvent).
    /// CpfCnpj e Address são nulos inicialmente — preenchidos pelo usuário posteriormente.
    /// </summary>
    public FinancialUser(Guid id, string name, string email)
    {
        Id = id;
        Name = name;
        Email = email;
        CpfCnpj = null;
        Address = null;
    }
}
