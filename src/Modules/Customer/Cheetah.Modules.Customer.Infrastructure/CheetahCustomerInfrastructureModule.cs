using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Customer.Domain;

namespace Cheetah.Modules.Customer.Infrastructure;

/// <summary>
/// Инфраструктура шаблонного модуля Customer: абстрактные базы EF
/// (<see cref="Persistence.CustomerDbContextBase{TContext,TCustomer,TContact}"/>,
/// <see cref="Persistence.Configurations.CustomerConfigurationBase{TCustomer}"/>) и generic-регистрация
/// через <c>AddCustomerInfrastructure&lt;TContext,TCustomer&gt;()</c>. Конкретный DbContext, конфигурацию
/// сущностей и миграции создаёт наследник.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CheetahCustomerDomainModule))]
public partial class CheetahCustomerInfrastructureModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
