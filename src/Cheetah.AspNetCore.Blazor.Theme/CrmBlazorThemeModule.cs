using Cheetah.Core;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.AspNetCore.Blazor.Theme;

[DependsOn(typeof(CoreModule))]
public partial class CrmBlazorThemeModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddOptions<ThemeOptions>().BindConfiguration(ThemeOptions.Section);
    }
}
