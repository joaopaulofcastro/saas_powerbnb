using MediatR;

namespace SaaS.PowerBnB.SharedKernel.Infrastructure.Communication.Events;

/// <summary>
/// Evento publicado pelo módulo Identity via Outbox após a criação de um UserProfile.
/// Consumido pelos módulos Charging e Financial para criar suas tabelas espelho.
/// O UserId é o valor do campo 'sub' do JWT emitido pelo Keycloak.
/// </summary>
public record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string Name,
    DateTime OccurredOn
) : INotification;
