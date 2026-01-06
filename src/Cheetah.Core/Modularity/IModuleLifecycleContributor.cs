namespace Cheetah.Core.Modularity;

public interface IModuleLifecycleContributor
{
    Task InitializeAsync(ApplicationInitializationContext context, ICrmModule module);

    void Initialize(ApplicationInitializationContext context, ICrmModule module);
    
    Task ShutdownAsync(ApplicationShutdownContext context, ICrmModule module);

    void Shutdown(ApplicationShutdownContext context, ICrmModule module);
}