using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core.Modules;

public abstract class CrmModule : ICrmModule
{
    public string? Name => GetType().FullName;
    public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        
    }
}