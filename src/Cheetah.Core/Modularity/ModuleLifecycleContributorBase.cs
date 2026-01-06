namespace Cheetah.Core.Modularity;

public abstract class ModuleLifecycleContributorBase : IModuleLifecycleContributor
{
    public virtual Task InitializeAsync(ApplicationInitializationContext context, ICrmModule module)
    {
        return Task.CompletedTask;
    }

    public virtual void Initialize(ApplicationInitializationContext context, ICrmModule module)
    {
    }

    public virtual Task ShutdownAsync(ApplicationShutdownContext context, ICrmModule module)
    {
        return Task.CompletedTask;
    }

    public virtual void Shutdown(ApplicationShutdownContext context, ICrmModule module)
    {
    }
}