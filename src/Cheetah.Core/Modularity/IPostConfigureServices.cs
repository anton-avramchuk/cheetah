namespace Cheetah.Core.Modularity;

public interface IPostConfigureServices
{
    void PostConfigureServices(ServiceConfigurationContext context);
}