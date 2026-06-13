using Cheetah.Core.Dapper.Repositories;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.Core.Dapper;

/// <summary>
/// Core, provider-independent Dapper data-access module: connection factory, SQL dialect contract,
/// entity mapping, specification-to-SQL translation, unit of work, repositories and query executor.
/// Reference a provider module (e.g. <c>CrmDapperPostgreSqlModule</c>) to supply the dialect and
/// connection provider.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(Core.Domain.CrmDomainModule))]
[DependsOn(typeof(Core.Specification.CrmSpecificationModule))]
[DependsOn(typeof(Core.DataAccess.CrmDataAccessModule))]
public partial class CrmDapperModule : CrmModule
{
    /// <inheritdoc />
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        // Open-generic Dapper repositories, resolvable for explicit per-entity binding.
        context.Services.TryAddScoped(typeof(DapperRepository<,>));
        context.Services.TryAddScoped(typeof(DapperRepository<>));
    }
}
