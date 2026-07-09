using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Expressions.JsonLogic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.FeatureManagement;

/// <summary>
/// Подключает движок фич-флагов: <see cref="IFeatureManager"/>, встроенные
/// <see cref="IFeatureFilter"/> и HTTP-builder контекста. Никакой БД.
/// Используйте в каждом модуле/микросервисе, где нужно спрашивать флаги.
/// <para>
/// Источник определений (<see cref="IFeatureDefinitionProvider"/>) подключается отдельно:
/// в монолите/сервисе FeatureManagement — Infrastructure (БД+кэш), в микросервисе-потребителе —
/// Client (<c>UseRemoteReplica</c>).
/// </para>
/// </summary>
[DependsOn(typeof(CoreModule), typeof(CrmExpressionsJsonLogicModule))]
public partial class CrmFeatureManagementModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        // Fallback: если хост не подключает ни Infrastructure (БД+кэш), ни Client (UseRemoteReplica),
        // IFeatureManager всё равно должен резолвиться (иначе валидация DI роняет хост, который лишь
        // транзитивно зависит от движка — например, через RequireFeature в Cheetah.Backend.Endpoints).
        // TryAdd — конкретный источник, если хост его подключает, регистрируется поверх обычным Add
        // и побеждает при резолве.
        context.Services.TryAddScoped<IFeatureDefinitionProvider, NullFeatureDefinitionProvider>();
    }
}
