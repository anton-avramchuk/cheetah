namespace Cheetah.Core.Inbox.Mongo;

/// <summary>
/// Configuration for the MongoDB inbox store: collection name and the logical connection-string
/// name (should match the consumer's connection so the idempotency record commits with its work).
/// </summary>
public sealed class MongoInboxStoreOptions
{
    /// <summary>Collection holding inbox idempotency records.</summary>
    public string InboxCollection { get; set; } = "InboxMessages";

    /// <summary>Logical connection-string name (<c>null</c> = default connection).</summary>
    public string? ConnectionName { get; set; }
}
