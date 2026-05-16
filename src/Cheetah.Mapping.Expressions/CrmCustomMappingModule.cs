using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Mapping.Expressions;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmMappingCoreModule))]
public partial class CrmCustomMappingModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Singleton — у маппера внутренний статический кэш скомпилированных делегатов.
        context.Services.AddSingleton<IObjectMapper, ExpressionObjectMapper>();
    }
}
