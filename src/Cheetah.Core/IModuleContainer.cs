using Cheetah.Core.Modules;

namespace Cheetah.Core;

public interface IModuleContainer
{

    IReadOnlyList<ICrmModuleDescriptor> Modules { get; }
}