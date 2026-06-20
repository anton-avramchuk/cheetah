using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Expressions.JsonLogic;

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
    }
}
