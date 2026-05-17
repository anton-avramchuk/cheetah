using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.FileStorage.Http;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmFileStorageModule))]
public partial class CrmFileStorageHttpModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;
        services.Configure<HttpFileStorageOptions>(services.GetConfiguration().GetSection("FileStorage:Http"));
        services.AddHttpClient(HttpFileStorage.HttpClientName);
        RegisterServices(services);
    }
}
