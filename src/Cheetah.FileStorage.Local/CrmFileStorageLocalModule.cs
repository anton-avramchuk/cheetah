using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.FileStorage.Local;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmFileStorageModule))]
public partial class CrmFileStorageLocalModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        services.Configure<LocalFileStorageOptions>(services.GetConfiguration().GetSection("FileStorage:Local"));
        RegisterServices(services);
    }
}
