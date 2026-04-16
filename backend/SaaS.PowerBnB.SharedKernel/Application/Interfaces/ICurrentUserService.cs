namespace SaaS.PowerBnB.SharedKernel.Application.Interfaces;

public interface ICurrentUserService
{
    // Será o 'sub' (Subject) do Token JWT do Keycloak
    string? UserId { get; }
}
