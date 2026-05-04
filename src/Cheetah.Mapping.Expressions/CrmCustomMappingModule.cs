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
        
        // Регистрация кастомного маппера как Singleton, 
        // так как он имеет внутренний статический кэш
        context.Services.AddSingleton<IObjectMapper, ExpressionObjectMapper>();
    }
}
