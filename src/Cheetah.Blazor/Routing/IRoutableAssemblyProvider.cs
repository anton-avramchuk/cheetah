using System.Reflection;

namespace Cheetah.Blazor.Routing;

/// <summary>
/// Provides assemblies containing routable components
/// </summary>
public interface IRoutableAssemblyProvider
{
    /// <summary>
    /// Gets all assemblies that contain routable Blazor components
    /// </summary>
    IEnumerable<Assembly> GetRoutableAssemblies();
}
