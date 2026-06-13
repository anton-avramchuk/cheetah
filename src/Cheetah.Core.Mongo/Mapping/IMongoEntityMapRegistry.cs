namespace Cheetah.Core.Mongo.Mapping;

/// <summary>
/// Resolves and caches <see cref="MongoEntityMapping"/> for entity types, using an explicitly
/// registered <see cref="IMongoEntityMap"/> when present and falling back to
/// <see cref="MongoMappingConventions"/> otherwise. Also guarantees the entity's
/// <see cref="MongoDB.Bson.Serialization.BsonClassMap"/> and the global conventions are registered.
/// </summary>
public interface IMongoEntityMapRegistry
{
    /// <summary>Gets the resolved mapping for <typeparamref name="TEntity"/>.</summary>
    MongoEntityMapping GetMapping<TEntity>() where TEntity : class;

    /// <summary>Gets the resolved mapping for the given entity type.</summary>
    MongoEntityMapping GetMapping(Type entityType);
}
