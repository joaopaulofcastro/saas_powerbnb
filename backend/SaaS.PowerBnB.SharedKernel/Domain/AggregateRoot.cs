using MediatR;

namespace SaaS.PowerBnB.SharedKernel.Domain;

public abstract class AggregateRoot : EntityBase
{
    private readonly List<INotification> _domainEvents = new();
    public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot() { }
    protected AggregateRoot(Guid id) : base(id) { }

    public void AddDomainEvent(INotification domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();
}