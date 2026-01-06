using Cheetah.Core.Extensions.Collections;

namespace Cheetah.Core.Domain;

/// <summary>
/// Base class for entities with a unique identifier
/// </summary>
public abstract class Entity<TId> : Entity, IEquatable<Entity<TId>>, IEntity<TId>
{
    public TId Id { get; protected set; } = default!;

    protected Entity()
    {
    }

    protected Entity(TId id) => Id = id;

    public override bool Equals(object? obj)
        => obj is Entity<TId> other && Equals(other);

    public bool Equals(Entity<TId>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);
    }

    public override int GetHashCode()
        => EqualityComparer<TId>.Default.GetHashCode(Id!);

    public override object?[] GetKeys()
    {
        return [Id];
    }

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
        => Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
        => !Equals(left, right);
}

public abstract class Entity : IEntity
{
    protected Entity()
    {
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[ENTITY: {GetType().Name}] Keys = {GetKeys().JoinAsString(", ")}";
    }

    public abstract object?[] GetKeys();
}