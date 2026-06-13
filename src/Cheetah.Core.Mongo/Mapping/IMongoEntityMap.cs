using MongoDB.Bson.Serialization;

namespace Cheetah.Core.Mongo.Mapping;

/// <summary>
/// Non-generic contract for an explicit entity-to-collection map. Implementations are registered
/// in DI as <c>[Export(Singleton, typeof(IMongoEntityMap))]</c> (typically by deriving
/// <see cref="MongoEntityMap{TEntity}"/>) and are discovered by <see cref="IMongoEntityMapRegistry"/>.
/// </summary>
public interface IMongoEntityMap
{
    /// <summary>The entity type this map applies to.</summary>
    Type EntityType { get; }

    /// <summary>Builds the resolved collection metadata.</summary>
    MongoEntityMapping BuildMapping();

    /// <summary>
    /// Builds the <see cref="BsonClassMap"/> for the entity, applying configured element-name
    /// overrides and ignores. <c>DomainEvents</c> is always unmapped.
    /// </summary>
    BsonClassMap BuildClassMap();
}
