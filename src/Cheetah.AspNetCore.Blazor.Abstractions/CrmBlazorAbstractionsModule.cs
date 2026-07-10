using Cheetah.Core;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.AspNetCore.Blazor.Abstractions;

[DependsOn(typeof(CoreModule))]
public partial class CrmBlazorAbstractionsModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        // Fallback: хост без движка фич-флагов видит все пункты меню и все страницы. Свою реализацию
        // поверх IFeatureManager хост регистрирует обычным Add и побеждает при резолве.
        context.Services.TryAddSingleton<IFeatureVisibilityEvaluator>(NullFeatureVisibilityEvaluator.Instance);
    }
}
