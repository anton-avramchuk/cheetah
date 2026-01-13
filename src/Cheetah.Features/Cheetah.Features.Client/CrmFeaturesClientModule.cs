using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Core.Tenants;
using Cheetah.Features.Application;
using Cheetah.Features.Contracts;
using Cheetah.Features.Shared;

namespace Cheetah.Features.Client;

/// <summary>
/// Features Client Module - Backend client for Features module
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmCQRSCoreModule))]
[DependsOn(typeof(CrmTenantsCoreModule))]
[DependsOn(typeof(CrmFeaturesApplicationModule))]
[DependsOn(typeof(CrmFeaturesSharedModule))]
[DependsOn(typeof(CrmFeaturesContractsModule))]
public partial class CrmFeaturesClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
