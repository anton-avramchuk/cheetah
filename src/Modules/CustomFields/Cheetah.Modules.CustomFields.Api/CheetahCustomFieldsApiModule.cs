using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Modules.CustomFields.Api.Endpoints;
using Cheetah.Modules.CustomFields.Application;
using Cheetah.Modules.CustomFields.Contracts;

namespace Cheetah.Modules.CustomFields.Api;

/// <summary>
/// Api-модуль CustomFields: маппит реестр/админку/значения/видимость в
/// <see cref="OnApplicationInitialization"/>. Только Minimal API (контроллеры запрещены).
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CheetahCustomFieldsApplicationModule),
    typeof(CheetahCustomFieldsContractsModule))]
public partial class CheetahCustomFieldsApiModule : CrmModule
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        new CustomFieldsEndpoints().Map(context.GetRouteBuilder());
    }
}
