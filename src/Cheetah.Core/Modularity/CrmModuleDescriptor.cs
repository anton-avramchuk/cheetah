using System.Collections.Immutable;
using System.Reflection;
using Cheetah.Core.Extensions.Collections;

namespace Cheetah.Core.Modularity;

public class CrmModuleDescriptor : ICrmModuleDescriptor
{
    public Type Type { get; }

    public Assembly Assembly { get; }

    public Assembly[] AllAssemblies { get; }

    public ICrmModule Instance { get; }

    public IReadOnlyList<ICrmModuleDescriptor> Dependencies => _dependencies.ToImmutableList();
    private readonly List<ICrmModuleDescriptor> _dependencies;

    public CrmModuleDescriptor(
        Type type,
        ICrmModule instance)
    {
        CrmModule.CheckCrmModuleType(type);

        if (!type.GetTypeInfo().IsAssignableFrom(instance.GetType()))
        {
            throw new ArgumentException($"Given module instance ({instance.GetType().AssemblyQualifiedName}) is not an instance of given module type: {type.AssemblyQualifiedName}");
        }

        Type = type;
        Assembly = type.Assembly;
        AllAssemblies = CrmModuleHelper.GetAllAssemblies(type);
        Instance = instance;

        _dependencies = new List<ICrmModuleDescriptor>();
    }

    public void AddDependency(ICrmModuleDescriptor descriptor)
    {
        _dependencies.AddIfNotContains(descriptor);
    }

    public override string ToString()
    {
        return $"[CrmModuleDescriptor {Type.FullName}]";
    }
}