using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Activities.Domain;

namespace Cheetah.Modules.Activities.Infrastructure;

/// <summary>
/// Инфраструктура шаблонного модуля Activities: абстрактные базы EF
/// (<see cref="Persistence.ActivitiesDbContextBase{TContext,TActivity}"/>,
/// <see cref="Persistence.Configurations.ActivityConfigurationBase{TActivity}"/>) и generic-регистрация
/// через <c>AddActivitiesInfrastructure&lt;TContext,TActivity&gt;()</c>. Конкретный DbContext,
/// конфигурацию сущности и миграции создаёт наследник.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CheetahActivitiesDomainModule))]
public partial class CheetahActivitiesInfrastructureModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
