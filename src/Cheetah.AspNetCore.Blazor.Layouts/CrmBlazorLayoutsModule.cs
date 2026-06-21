using Cheetah.AspNetCore.Blazor.Abstractions;
using Cheetah.AspNetCore.Blazor.Dialogs;
using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.AspNetCore.Blazor.Toast;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.AspNetCore.Blazor.Layouts;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorAbstractionsModule))]
[DependsOn(typeof(CrmBlazorNavigationModule))]
[DependsOn(typeof(CrmBlazorDialogsModule))]
[DependsOn(typeof(CrmBlazorToastModule))]
public class CrmBlazorLayoutsModule : CrmModule
{
}
