using Cheetah.Core.Mongo.Mapping;
using Cheetah.Core.Mongo.Repositories;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.Core.Mongo;

/// <summary>
/// MongoDB data-access module: client/database providers, entity mapping and conventions,
/// specification-to-filter translation, unit of work, repositories and query executor. Implements
/// the shared <c>IRepository&lt;,&gt;</c> contracts so the application layer is unchanged.
/// </summary>
/// <remarks>
/// MongoDB ships a single official driver, so there is no provider-specific sub-package (unlike the
/// Dapper SQL stack). Multi-document transactions require a replica set or sharded cluster.
/// </remarks>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(Core.Domain.CrmDomainModule))]
[DependsOn(typeof(Core.Specification.CrmSpecificationModule))]
[DependsOn(typeof(Core.DataAccess.CrmDataAccessModule))]
public partial class CrmMongoModule : CrmModule
{
    /// <inheritdoc />
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register conventions before any (de)serialization happens, including DomainEvents ignore.
        MongoMappingConventions.EnsureRegistered();

        RegisterServices(context.Services);

        // Open-generic Mongo repositories, resolvable for explicit per-entity binding.
        context.Services.TryAddScoped(typeof(MongoRepository<,>));
        context.Services.TryAddScoped(typeof(MongoRepository<>));
    }
}
