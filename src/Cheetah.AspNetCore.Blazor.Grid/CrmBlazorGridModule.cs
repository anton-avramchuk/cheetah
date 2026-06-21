using Cheetah.AspNetCore.Blazor.Dialogs;
using Cheetah.AspNetCore.Blazor.Toast;
using Cheetah.Core;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;

namespace Cheetah.AspNetCore.Blazor.Grid;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmGridModule))]
[DependsOn(typeof(CrmBlazorDialogsModule))]
[DependsOn(typeof(CrmBlazorToastModule))]
public class CrmBlazorGridModule : CrmModule
{
}
