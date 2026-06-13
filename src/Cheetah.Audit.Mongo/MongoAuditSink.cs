using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Mongo.UnitOfWork;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Cheetah.Audit.Mongo;

/// <summary>
/// MongoDB <see cref="IAuditSink"/>. Buffers the audit entries on the scoped
/// <see cref="IMongoUnitOfWork"/> so they commit in the same transaction as the business aggregate
/// (the interceptor invokes the sink before the unit of work flushes), mirroring the EF sink.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IAuditSink))]
public class MongoAuditSink : IAuditSink
{
    private readonly IMongoUnitOfWork _unitOfWork;
    private readonly MongoAuditStoreOptions _options;

    /// <summary>Initializes a new instance of the <see cref="MongoAuditSink"/> class.</summary>
    public MongoAuditSink(IMongoUnitOfWork unitOfWork, IOptions<MongoAuditStoreOptions> options)
    {
        _unitOfWork = unitOfWork;
        _options = options.Value;
    }

    /// <inheritdoc />
    public ValueTask EmitAsync(IReadOnlyList<AuditEntry> entries, CancellationToken cancellationToken = default)
    {
        if (entries.Count == 0)
            return ValueTask.CompletedTask;

        _unitOfWork.Enqueue(new PendingWrite
        {
            ConnectionName = _options.ConnectionName,
            ApplyAsync = async (db, session, ct) =>
            {
                await db.GetCollection<AuditEntry>(_options.AuditCollection)
                    .InsertManyAsync(session, entries, cancellationToken: ct);
                return entries.Count;
            },
        });
        return ValueTask.CompletedTask;
    }
}
