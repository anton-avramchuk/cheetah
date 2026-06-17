using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Cheetah.FileStorage;
using Cheetah.Mapping.Core;
using Cheetah.Modules.SalesDocuments.Domain;

namespace Cheetah.Modules.SalesDocuments.Infrastructure;

/// <summary>
/// Инфраструктура шаблонного модуля SalesDocuments: абстрактные базы EF
/// (<see cref="Persistence.SalesDocumentsDbContextBase{TContext,TDoc}"/>,
/// <see cref="Persistence.Configurations.SalesDocumentConfigurationBase{TDoc}"/>), generic-регистрация
/// через <c>AddSalesDocumentsInfrastructure&lt;TContext,TDoc&gt;()</c>, генератор номеров, PDF-сервис.
/// Конкретный DbContext, конфигурацию документа и миграции создаёт наследник.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmGridModule),
    typeof(CrmMappingCoreModule),
    typeof(CrmFileStorageModule),
    typeof(CheetahSalesDocumentsDomainModule))]
public partial class CheetahSalesDocumentsInfrastructureModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
