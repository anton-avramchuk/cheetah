namespace Cheetah.Core.Domain.Exceptions;

public class EntityNotFoundException : Exception
{
    public string EntityType { get; }
    public object? EntityId { get; }

    public EntityNotFoundException(string entityType, object? entityId = null)
        : base($"Entity '{entityType}' with id '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public EntityNotFoundException(string entityType, object? entityId, string message)
        : base(message)
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public static EntityNotFoundException For<TEntity>(object? id = null)
        => new(typeof(TEntity).Name, id);
}
