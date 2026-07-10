using Cheetah.AspNetCore.Blazor.Abstractions;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.AspNetCore.Blazor.Navigation;

// Дефолтный IFeatureVisibilityEvaluator (все фичи включены) регистрирует CrmBlazorAbstractionsModule.
[DependsOn(typeof(CoreModule), typeof(CrmBlazorAbstractionsModule))]
public partial class CrmBlazorNavigationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
