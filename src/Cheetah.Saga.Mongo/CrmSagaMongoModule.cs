using Cheetah.Core;
using Cheetah.Core.Mongo;
using Cheetah.Core.Mongo.Connections;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Cheetah.Saga.Mongo;

/// <summary>
/// MongoDB implementation of <see cref="ISagaRepository"/>. <see cref="MongoSagaRepository"/> is
/// auto-registered via <c>[Export]</c>; configure collection/connection through
/// <see cref="MongoSagaStoreOptions"/>. Creates the unique <c>(SagaType, CorrelationKey)</c> index
/// on application initialization.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmMongoModule), typeof(CrmSagaModule))]
public partial class CrmSagaMongoModule : CrmModule
{
    /// <inheritdoc />
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }

    /// <inheritdoc />
    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        var options = context.ServiceProvider.GetRequiredService<IOptions<MongoSagaStoreOptions>>().Value;
        var databaseProvider = context.ServiceProvider.GetRequiredService<IMongoDatabaseProvider>();
        var database = await databaseProvider.GetDatabaseAsync(options.ConnectionName);

        var sagas = database.GetCollection<SagaInstance>(options.SagaCollection);
        await sagas.Indexes.CreateOneAsync(new CreateIndexModel<SagaInstance>(
            Builders<SagaInstance>.IndexKeys
                .Ascending(x => x.SagaType)
                .Ascending(x => x.CorrelationKey),
            new CreateIndexOptions { Unique = true }));
    }
}
