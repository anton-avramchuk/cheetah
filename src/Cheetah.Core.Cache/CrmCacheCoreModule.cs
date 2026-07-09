using Cheetah.Core;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.Core.Cache;

[DependsOn(typeof(CoreModule))]
public class CrmCacheCoreModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Дефолт: процесс-локальный IMemoryCache. TryAdd — хост, которому нужен распределённый кэш,
        // может зарегистрировать свою реализацию ICacheService ДО инициализации модулей (Start()),
        // и она победит (TryAdd не перезапишет уже существующую регистрацию).
        context.Services.AddMemoryCache();
        context.Services.TryAddSingleton<ICacheService, MemoryCacheService>();
    }
}