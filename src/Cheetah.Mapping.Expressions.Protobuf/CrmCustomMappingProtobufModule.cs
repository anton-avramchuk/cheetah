using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Mapping.Expressions.Protobuf;

/// <summary>
/// Opt-in модуль: включает поддержку proto-типов в самописном мапере (<c>CrmCustomMappingModule</c>).
/// Подключать в приложениях, которые используют gRPC вместе с <c>ExpressionObjectMapper</c>.
/// Приложения на Mapster используют вместо этого <c>CrmBackendGrpcMapsterModule</c>.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmCustomMappingModule))]
public partial class CrmCustomMappingProtobufModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        ProtobufMappingSupport.Register();
    }
}
