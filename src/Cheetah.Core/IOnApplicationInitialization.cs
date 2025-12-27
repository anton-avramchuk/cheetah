namespace Cheetah.Core;

public interface IOnApplicationInitialization
{
    Task OnApplicationInitializationAsync(ApplicationInitializationContext context);

    void OnApplicationInitialization(ApplicationInitializationContext context);
}