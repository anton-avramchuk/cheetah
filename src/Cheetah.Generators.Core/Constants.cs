using Cheetah.Core;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Modularity;

namespace Cheetah.Generators.Core;

public static class Constants
{
    public static string? BootstrapperAttributeName = typeof(BootstrapperAttribute).FullName;

    public static string? ModuleTypeName = typeof(ICrmModule).FullName;

    public static string? ExportAttributeName = typeof(ExportAttribute).FullName;


    public static string? DependsOnAttributeName = typeof(DependsOnAttribute).FullName;
}