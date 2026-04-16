using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using SaaS.PowerBnB.SharedKernel.Domain;
using SaaS.PowerBnB.SharedKernel.Infrastructure.Outbox;

namespace SaaS.PowerBnB.SharedKernel.Infrastructure.Interceptors;

public sealed class InsertOutboxMessagesInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context is null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        // 1. Pega todas as entidades rastreadas que tenham herdado de AggregateRoot
        var entities = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        // 2. Extrai os eventos e limpa as coleções das entidades originais
        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        // 3. Converte para a entidade OutboxMessage
        var outboxMessages = domainEvents.Select(domainEvent => new OutboxMessage(
            id: Guid.NewGuid(),
            type: domainEvent.GetType().Name,
            content: JsonConvert.SerializeObject(domainEvent, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All // Vital para o MediatR saber qual tipo desserializar depois
            }),
            occurredOnUtc: DateTime.UtcNow
        )).ToList();

        // 4. Adiciona ao contexto (será salvo na mesma transação atômica do negócio)
        context.Set<OutboxMessage>().AddRange(outboxMessages);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
