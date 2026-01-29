namespace Cheetah.Core.Modularity;

public static class ModuleInitializer
{
    private static readonly List<Type> modules = [];
    public static void AddModule<TModule>() where TModule : ICrmModule
    {
        modules.Add(typeof(TModule));
    }

    public static IEnumerable<Type> Modules => modules.AsEnumerable();
}