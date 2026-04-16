namespace SaaS.PowerBnB.SharedKernel.Infrastructure.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }
    public string Type { get; private set; } = string.Empty; // Nome da classe do evento
    public string Content { get; private set; } = string.Empty; // O payload em JSON
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime? ProcessedOnUtc { get; private set; }
    public string? Error { get; private set; }

    // Construtor para o EF Core
    private OutboxMessage() { }

    public OutboxMessage(Guid id, string type, string content, DateTime occurredOnUtc)
    {
        Id = id;
        Type = type;
        Content = content;
        OccurredOnUtc = occurredOnUtc;
    }

    public void MarkAsProcessed()
    {
        ProcessedOnUtc = DateTime.UtcNow;
    }

    public void MarkAsFailed(string error)
    {
        Error = error;
    }
}