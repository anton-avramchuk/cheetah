using System.Reflection;

namespace Cheetah.Core.Modularity;

public interface ICrmModuleDescriptor
{
    /// <summary>
    /// Type of the module class.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// Main assembly that defines the module <see cref="Type"/>.
    /// </summary>
    Assembly Assembly { get; }

    Assembly[] AllAssemblies { get; }

    /// <summary>
    /// The instance of the module class (singleton).
    /// </summary>
    ICrmModule Instance { get; }


    /// <summary>
    /// Modules on which this module depends on.
    /// A module can depend on another module using the <see cref="DependsOnAttribute"/> attribute.
    /// </summary>
    IReadOnlyList<ICrmModuleDescriptor> Dependencies { get; }
}