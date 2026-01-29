using System.Reflection;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Modularity;

namespace Cheetah.Blazor.Routing;

[Export(LifetimeType.Singleton, typeof(IRoutableAssemblyProvider))]
public class RoutableAssemblyProvider : IRoutableAssemblyProvider
{
    private readonly Lazy<Assembly[]> _assemblies;

    public RoutableAssemblyProvider()
    {
        _assemblies = new Lazy<Assembly[]>(CollectAssemblies);
    }

    public IEnumerable<Assembly> GetRoutableAssemblies()
    {
        return _assemblies.Value;
    }

    private static Assembly[] CollectAssemblies()
    {
        var assemblies = new HashSet<Assembly>();

        foreach (var moduleType in ModuleInitializer.Modules)
        {
            var assembly = moduleType.Assembly;

            // Check if assembly has any routable components (has @page directive)
            var hasRoutableComponents = assembly.GetTypes()
                .Any(t => t.GetCustomAttributes()
                    .Any(a => a.GetType().FullName == "Microsoft.AspNetCore.Components.RouteAttribute"));

            if (hasRoutableComponents)
            {
                assemblies.Add(assembly);
            }
        }

        return assemblies.ToArray();
    }
}
