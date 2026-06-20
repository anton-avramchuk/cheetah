using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Api.Endpoints;
using Cheetah.Modules.FeatureManagement.Application;
using Cheetah.Modules.FeatureManagement.Contracts;

namespace Cheetah.Modules.FeatureManagement.Api;

/// <summary>
/// Абстрактный базовый Api-модуль FeatureManagement: маппит реестр/админку/оценку в
/// <see cref="OnApplicationInitialization"/>. Наследник закрывает generic своими типами:
/// <code>
/// public sealed class AppFeatureApiModule
///     : CheetahFeatureManagementApiModuleBase&lt;FeatureFlagEndpoints, CreateFeatureFlagRequest, FeatureFlagDto&gt; { }
/// </code>
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CrmFeatureManagementModule),
    typeof(CheetahFeatureManagementApplicationModule),
    typeof(CheetahFeatureManagementContractsModule))]
public abstract class CheetahFeatureManagementApiModuleBase<TEndpoints, TCreateRequest, TDto> : CrmModule
    where TEndpoints : FeatureFlagEndpointsBase<TCreateRequest, TDto>, new()
    where TCreateRequest : CreateFeatureFlagRequestBase
    where TDto : FeatureFlagDtoBase
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        new TEndpoints().Map(context.GetRouteBuilder());
    }
}
