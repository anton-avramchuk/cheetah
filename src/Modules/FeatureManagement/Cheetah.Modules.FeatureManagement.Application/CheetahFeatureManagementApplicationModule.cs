using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Contracts;
using Cheetah.Modules.FeatureManagement.Domain;
using Cheetah.Modules.FeatureManagement.DomainEvents;

namespace Cheetah.Modules.FeatureManagement.Application;

/// <summary>
/// Прикладной слой шаблонного модуля FeatureManagement: generic CQRS флагов (создание, kill-switch,
/// таргетинг, tenant-override, registry/sync) + батч-оценка через <c>IFeatureManager</c>. Закрытые
/// generic-handler'ы регистрирует наследник через <c>AddFeatureManagementApplication&lt;…&gt;()</c>.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmEventsCoreModule),
    typeof(CrmFeatureManagementModule),
    typeof(CheetahFeatureManagementDomainModule),
    typeof(CheetahFeatureManagementContractsModule),
    typeof(CheetahFeatureManagementDomainEventsModule))]
public partial class CheetahFeatureManagementApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
