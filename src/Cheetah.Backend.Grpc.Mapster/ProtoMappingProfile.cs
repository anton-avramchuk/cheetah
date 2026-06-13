using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Google.Protobuf.WellKnownTypes;
using Mapster;

namespace Cheetah.Backend.Grpc.Mapster;

/// <summary>
/// Универсальные правила конвертации между .NET-типами и proto well-known types.
/// Специфичные для модуля правила (например <c>Guid &lt;-&gt; string</c> для конкретного
/// <c>GuidProto</c>) задаются в профиле соответствующего модуля.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public sealed class ProtoMappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<DateTime, Timestamp>()
            .MapWith(src => Timestamp.FromDateTime(DateTime.SpecifyKind(src, DateTimeKind.Utc)));

        config.NewConfig<Timestamp, DateTime>()
            .MapWith(src => src.ToDateTime());

        config.NewConfig<DateTimeOffset, Timestamp>()
            .MapWith(src => Timestamp.FromDateTimeOffset(src));

        config.NewConfig<Timestamp, DateTimeOffset>()
            .MapWith(src => src.ToDateTimeOffset());
    }
}
