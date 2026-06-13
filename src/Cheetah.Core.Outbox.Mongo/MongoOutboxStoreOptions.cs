namespace Cheetah.Core.Outbox.Mongo;

/// <summary>
/// Configuration for the MongoDB outbox/dead-letter stores: collection names and the logical
/// connection-string name. The connection name should match the one the aggregate writes to so the
/// outbox insert commits in the same transaction.
/// </summary>
public sealed class MongoOutboxStoreOptions
{
    /// <summary>Collection holding pending/processed outbox messages.</summary>
    public string OutboxCollection { get; set; } = "OutboxMessages";

    /// <summary>Collection holding dead-lettered messages.</summary>
    public string DeadLetterCollection { get; set; } = "DeadLetterMessages";

    /// <summary>Logical connection-string name (<c>null</c> = default connection).</summary>
    public string? ConnectionName { get; set; }
}
