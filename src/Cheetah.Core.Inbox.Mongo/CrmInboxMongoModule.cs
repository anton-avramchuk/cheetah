using Cheetah.Core.Mongo;
using Cheetah.Core.Mongo.Connections;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Cheetah.Core.Inbox.Mongo;

/// <summary>
/// MongoDB support for the Inbox store. <see cref="MongoInboxStore"/> is auto-registered via
/// <c>[Export]</c>; configure collection/connection through <see cref="MongoInboxStoreOptions"/>.
/// Creates the unique <c>(EventId, ConsumerName)</c> index on application initialization.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmMongoModule), typeof(CrmInboxModule))]
public partial class CrmInboxMongoModule : CrmModule
{
    /// <inheritdoc />
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }

    /// <inheritdoc />
    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var options = context.ServiceProvider.GetRequiredService<IOptions<MongoInboxStoreOptions>>().Value;
        var databaseProvider = context.ServiceProvider.GetRequiredService<IMongoDatabaseProvider>();
        var database = await databaseProvider.GetDatabaseAsync(options.ConnectionName);

        var inbox = database.GetCollection<InboxMessage>(options.InboxCollection);
        await inbox.Indexes.CreateOneAsync(new CreateIndexModel<InboxMessage>(
            Builders<InboxMessage>.IndexKeys
                .Ascending(x => x.EventId)
                .Ascending(x => x.ConsumerName),
            new CreateIndexOptions { Unique = true }));
    }
}
