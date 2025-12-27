using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cheetah.Core;

public class CrmApplicationCreationOptions
{
    
    public IServiceCollection Services { get; }



    /// <summary>
    /// The options in this property only take effect when IConfiguration not registered.
    /// </summary>

    public ConfigurationBuilderOptions Configuration { get; }

    public bool SkipConfigureServices { get; set; }

    public string? ApplicationName { get; set; }

    //todo: from config
    public string? Environment { get; set; } = Environments.Development;

    public CrmApplicationCreationOptions(IServiceCollection services)
    {
        Services = services;
        Configuration = new ConfigurationBuilderOptions();
    }
}