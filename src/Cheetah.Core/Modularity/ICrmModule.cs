namespace Cheetah.Core.Modularity;

/// <summary>
/// Base interface for all CRM modules
/// </summary>
public interface ICrmModule
{
    void ConfigureServices(ServiceConfigurationContext context);
}