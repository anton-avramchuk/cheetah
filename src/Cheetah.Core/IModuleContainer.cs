using Cheetah.Core.Modularity;

namespace Cheetah.Core;

public interface IModuleContainer
{

    IReadOnlyList<ICrmModuleDescriptor> Modules { get; }
}