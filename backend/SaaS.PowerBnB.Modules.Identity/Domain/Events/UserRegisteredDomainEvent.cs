using MediatR;

namespace SaaS.PowerBnB.Modules.Identity.Domain.Events;

/// <summary>
/// Evento de domínio interno ao agregado UserProfile.
/// Capturado pelo InsertOutboxMessagesInterceptor e gravado na tabela identity.outbox_messages.
/// O IdentityOutboxWorker o desserializa e publica como UserRegisteredEvent (SharedKernel).
/// </summary>
internal record UserRegisteredDomainEvent(
    Guid UserId,
    string Email,
    string Name,
    DateTime OccurredOn
) : INotification;
