using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Catalog.Domain;

namespace Cheetah.Modules.Catalog.Infrastructure;

/// <summary>
/// Инфраструктура шаблонного модуля Catalog: абстрактные базы EF
/// (<see cref="Persistence.CatalogDbContextBase{TContext,TProduct}"/>,
/// <see cref="Persistence.Configurations.ProductConfigurationBase{TProduct}"/>) и generic-регистрация
/// через <c>AddCatalogInfrastructure&lt;TContext,TProduct&gt;()</c>. Конкретный DbContext,
/// конфигурацию товара и миграции создаёт наследник.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmGridModule),
    typeof(CrmMappingCoreModule),
    typeof(CheetahCatalogDomainModule))]
public partial class CheetahCatalogInfrastructureModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
