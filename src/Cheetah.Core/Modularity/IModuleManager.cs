namespace Cheetah.Core.Modularity;

public interface IModuleManager
{
    Task InitializeModulesAsync(ApplicationInitializationContext context);

    void InitializeModules(ApplicationInitializationContext context);
}