namespace SaaS.PowerBnB.SharedKernel.Domain;

public abstract class EntityBase : IEquatable<EntityBase>
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    
    // Auditoria
    public DateTimeOffset CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "system";
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }


    protected EntityBase() { }
    protected EntityBase(Guid id) => Id = id;


    public bool Equals(EntityBase? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        if (Id == Guid.Empty || other.Id == Guid.Empty)
            return false;

        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not EntityBase other)
            return false;

        return Equals(other);
    }

    public override int GetHashCode()
    {
        return (GetType().GetHashCode() * 907) + Id.GetHashCode();
    }

    public static bool operator ==(EntityBase? a, EntityBase? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(EntityBase? a, EntityBase? b)
    {
        return !(a == b);
    }
}