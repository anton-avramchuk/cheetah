namespace Cheetah.Saga.Mongo;

/// <summary>
/// Configuration for the MongoDB saga repository: collection name and the logical connection-string
/// name.
/// </summary>
public sealed class MongoSagaStoreOptions
{
    /// <summary>Collection holding saga instances.</summary>
    public string SagaCollection { get; set; } = "SagaInstances";

    /// <summary>Logical connection-string name (<c>null</c> = default connection).</summary>
    public string? ConnectionName { get; set; }
}
