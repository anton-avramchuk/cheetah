using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Leads.Domain;

namespace Cheetah.Modules.Leads.Infrastructure;

/// <summary>
/// Инфраструктура шаблонного модуля Leads: абстрактные базы EF
/// (<see cref="Persistence.LeadsDbContextBase{TContext,TLead}"/>,
/// <see cref="Persistence.Configurations.LeadConfigurationBase{TLead}"/>) и generic-регистрация через
/// <c>AddLeadsInfrastructure&lt;TContext,TLead&gt;()</c>. Конкретный DbContext, конфигурацию сущности и
/// миграции создаёт наследник.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CheetahLeadsDomainModule))]
public partial class CheetahLeadsInfrastructureModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
