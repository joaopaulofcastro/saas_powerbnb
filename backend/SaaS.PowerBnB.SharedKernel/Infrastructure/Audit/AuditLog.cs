namespace SaaS.PowerBnB.SharedKernel.Audit;

public class AuditLog
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string TableName { get; private set; } = string.Empty;
    public Guid RecordId { get; private set; }
    public string Action { get; private set; } = string.Empty; // "Added", "Modified", "Deleted"
    public string? OldValues { get; private set; } // JSON
    public string? NewValues { get; private set; } // JSON
    public string UserId { get; private set; } = string.Empty;
    public DateTimeOffset Timestamp { get; private set; } = DateTimeOffset.UtcNow;

    protected AuditLog() { }

    public AuditLog(string tableName, Guid recordId, string action, string? oldValues, string? newValues, string userId)
    {
        TableName = tableName;
        RecordId = recordId;
        Action = action;
        OldValues = oldValues;
        NewValues = newValues;
        UserId = userId;
    }
}