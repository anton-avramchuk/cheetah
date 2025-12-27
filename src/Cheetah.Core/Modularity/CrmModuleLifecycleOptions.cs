using Cheetah.Core.Collections;

namespace Cheetah.Core.Modularity;

public class CrmModuleLifecycleOptions
{
    public ITypeList<IModuleLifecycleContributor> Contributors { get; } = new TypeList<IModuleLifecycleContributor>();
}