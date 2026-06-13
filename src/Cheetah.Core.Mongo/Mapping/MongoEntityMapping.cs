namespace Cheetah.Core.Mongo.Mapping;

/// <summary>
/// Resolved, immutable collection metadata for an entity type. Produced by an explicit
/// <see cref="IMongoEntityMap"/> or by convention, and cached by <see cref="IMongoEntityMapRegistry"/>.
/// </summary>
public sealed class MongoEntityMapping
{
    /// <summary>The mapped CLR entity type.</summary>
    public required Type EntityType { get; init; }

    /// <summary>The MongoDB collection name.</summary>
    public required string CollectionName { get; init; }

    /// <summary>
    /// The CLR name of the key property (mapped to <c>_id</c>). Defaults to <c>Id</c>.
    /// </summary>
    public required string KeyMemberName { get; init; }
}
