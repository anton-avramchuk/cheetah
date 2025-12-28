namespace Cheetah.Core.Modularity;

public interface IPreConfigureServices
{
    void PreConfigureServices(ServiceConfigurationContext context);
}