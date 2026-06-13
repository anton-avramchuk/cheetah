using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Mapster;

namespace Cheetah.Backend.Grpc.Mapster;

/// <summary>
/// Opt-in модуль: добавляет Mapster-правила конвертации .NET ↔ proto well-known types.
/// Подключать в приложениях, которые используют gRPC вместе с Mapster (<c>CrmMapsterModule</c>).
/// Приложения на самописном маппере (<c>CrmCustomMappingModule</c>) этот модуль не используют —
/// proto-конверсии для него нужно реализовывать средствами самого маппера.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmMapsterModule))]
public partial class CrmBackendGrpcMapsterModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
