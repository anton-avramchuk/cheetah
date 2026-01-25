using Cheetah.AspNetCore.Contracts.ModelBinders;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.AspNetCore.Contracts;

[DependsOn(typeof(CoreModule))]
public class CrmAspNetCoreContractsModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<MvcOptions>(options =>
        {
            options.ModelBinderProviders.Insert(0, new GridRequestModelBinderProvider());
        });
    }
}