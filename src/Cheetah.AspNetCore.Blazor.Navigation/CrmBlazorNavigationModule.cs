using Cheetah.Core;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.AspNetCore.Blazor.Navigation;

[DependsOn(typeof(CoreModule))]
public partial class CrmBlazorNavigationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        // Fallback: хост без движка фич-флагов видит все пункты меню. Свою реализацию поверх
        // IFeatureManager хост регистрирует обычным Add и побеждает при резолве.
        context.Services.TryAddSingleton<IMenuFeatureEvaluator>(NullMenuFeatureEvaluator.Instance);
    }
}
