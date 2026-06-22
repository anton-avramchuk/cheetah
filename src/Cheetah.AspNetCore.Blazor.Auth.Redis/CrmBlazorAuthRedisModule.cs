using Cheetah.AspNetCore.Blazor.Auth.Tokens;
using Cheetah.Backend.Redis;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.AspNetCore.Blazor.Auth.Redis;

/// <summary>
/// Подключает Redis-стор токенов вместо дефолтного in-memory: при наличии этого модуля
/// в графе <see cref="IUserTokenStore"/> резолвится в <see cref="RedisUserTokenStore"/>.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorAuthModule))]
[DependsOn(typeof(CrmBackendRedisModule))]
public partial class CrmBlazorAuthRedisModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services
            .AddOptions<RedisUserTokenStoreOptions>()
            .BindConfiguration(RedisUserTokenStoreOptions.SectionName);

        // Заменяем дефолтный in-memory стор (Singleton) на Redis (Scoped, как IRedisClient).
        context.Services.Replace(ServiceDescriptor.Scoped<IUserTokenStore, RedisUserTokenStore>());
    }
}
