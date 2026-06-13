namespace Cheetah.Audit.Mongo;

/// <summary>
/// Configuration for the MongoDB audit sink/publish store: collection name and the logical
/// connection-string name (should match the audited aggregate's connection so audit entries commit
/// in the same transaction).
/// </summary>
public sealed class MongoAuditStoreOptions
{
    /// <summary>Collection holding audit entries.</summary>
    public string AuditCollection { get; set; } = "AuditEntries";

    /// <summary>Logical connection-string name (<c>null</c> = default connection).</summary>
    public string? ConnectionName { get; set; }
}
