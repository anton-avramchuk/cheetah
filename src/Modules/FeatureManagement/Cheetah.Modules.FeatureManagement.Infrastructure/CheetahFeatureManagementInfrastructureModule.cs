using Cheetah.Core;
using Cheetah.Core.Cache;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.Modularity;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Domain;
using Cheetah.Modules.FeatureManagement.DomainEvents;

namespace Cheetah.Modules.FeatureManagement.Infrastructure;

/// <summary>
/// Инфраструктура шаблонного модуля FeatureManagement: абстрактные базы EF
/// (<see cref="Persistence.FeatureManagementDbContextBase{TContext,TFlag}"/>,
/// <see cref="Persistence.Configurations.FeatureFlagConfigurationBase{TFlag}"/>), реализация порта
/// <c>IFeatureDefinitionProvider</c> (БД+кэш) и инвалидатор. Generic-регистрация —
/// <c>AddFeatureManagementInfrastructure&lt;TContext,TFlag&gt;()</c>. Конкретный DbContext,
/// конфигурацию и миграции создаёт наследник / <c>.Default</c>.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmDomainModule),
    typeof(CrmCacheCoreModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CrmFeatureManagementModule),
    typeof(CheetahFeatureManagementDomainModule),
    typeof(CheetahFeatureManagementDomainEventsModule))]
public partial class CheetahFeatureManagementInfrastructureModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
