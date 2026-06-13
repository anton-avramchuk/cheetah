using Cheetah.Core;
using Cheetah.Core.Mongo;
using Cheetah.Core.Mongo.Connections;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Cheetah.Audit.Mongo;

/// <summary>
/// MongoDB support for the audit sink and publish store. <see cref="MongoAuditSink"/> and
/// <see cref="MongoAuditPublishStore"/> are auto-registered via <c>[Export]</c>; configure
/// collection/connection through <see cref="MongoAuditStoreOptions"/>. Creates the publisher index
/// on application initialization.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmMongoModule), typeof(CrmAuditModule))]
public partial class CrmAuditMongoModule : CrmModule
{
    /// <inheritdoc />
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }

    /// <inheritdoc />
    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var options = context.ServiceProvider.GetRequiredService<IOptions<MongoAuditStoreOptions>>().Value;
        var databaseProvider = context.ServiceProvider.GetRequiredService<IMongoDatabaseProvider>();
        var database = await databaseProvider.GetDatabaseAsync(options.ConnectionName);

        var audit = database.GetCollection<AuditEntry>(options.AuditCollection);
        // Drives ClaimPendingAsync (PublishedAt == null, NextAttemptAt <= now, ordered by OccurredAt).
        await audit.Indexes.CreateOneAsync(new CreateIndexModel<AuditEntry>(
            Builders<AuditEntry>.IndexKeys
                .Ascending(x => x.PublishedAt)
                .Ascending(x => x.NextAttemptAt)
                .Ascending(x => x.OccurredAt)));
    }
}
