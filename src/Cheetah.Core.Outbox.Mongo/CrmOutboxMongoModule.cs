using Cheetah.Core.Mongo;
using Cheetah.Core.Mongo.Connections;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Cheetah.Core.Outbox.Mongo;

/// <summary>
/// MongoDB support for the Outbox/DeadLetter stores. <see cref="MongoOutboxStore"/> and
/// <see cref="MongoDeadLetterStore"/> are auto-registered via <c>[Export]</c>; configure collection
/// and connection names through <see cref="MongoOutboxStoreOptions"/>. Indexes are created on
/// application initialization.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmMongoModule), typeof(CrmOutboxModule))]
public partial class CrmOutboxMongoModule : CrmModule
{
    /// <inheritdoc />
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }

    /// <inheritdoc />
    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var options = context.ServiceProvider.GetRequiredService<IOptions<MongoOutboxStoreOptions>>().Value;
        var databaseProvider = context.ServiceProvider.GetRequiredService<IMongoDatabaseProvider>();
        var database = await databaseProvider.GetDatabaseAsync(options.ConnectionName);

        var outbox = database.GetCollection<OutboxMessage>(options.OutboxCollection);
        // Drives GetPendingAsync (ProcessedAt == null, NextAttemptAt <= now, ordered by OccurredAt).
        await outbox.Indexes.CreateOneAsync(new CreateIndexModel<OutboxMessage>(
            Builders<OutboxMessage>.IndexKeys
                .Ascending(x => x.ProcessedAt)
                .Ascending(x => x.NextAttemptAt)
                .Ascending(x => x.OccurredAt)));
    }
}
