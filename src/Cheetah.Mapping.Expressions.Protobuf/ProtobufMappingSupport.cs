using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Cheetah.Mapping.Expressions.Protobuf;

/// <summary>
/// Регистрирует в <see cref="ExpressionBuilder"/> поддержку proto-типов для самописного маппера:
/// конверсии well-known types (Timestamp) и null-safe сборку proto-сообщений (<see cref="IMessage"/>).
/// Generic-конверсии (Guid&lt;-&gt;string) и маппинг через конструктор уже встроены в ядро.
/// </summary>
public static class ProtobufMappingSupport
{
    private static readonly object _gate = new();
    private static bool _registered;

    /// <summary>Идемпотентно регистрирует proto-поддержку. Безопасно вызывать многократно.</summary>
    public static void Register()
    {
        if (_registered) return;
        lock (_gate)
        {
            if (_registered) return;

            ExpressionBuilder.RegisterConverter<DateTime, Timestamp>(
                d => Timestamp.FromDateTime(DateTime.SpecifyKind(d, DateTimeKind.Utc)));
            ExpressionBuilder.RegisterConverter<Timestamp, DateTime>(t => t.ToDateTime());
            ExpressionBuilder.RegisterConverter<DateTimeOffset, Timestamp>(d => Timestamp.FromDateTimeOffset(d));
            ExpressionBuilder.RegisterConverter<Timestamp, DateTimeOffset>(t => t.ToDateTimeOffset());

            // proto-сообщения собираем null-safe (string-поля не получают null) + допускаем обёртку скаляра.
            ExpressionBuilder.RegisterNullSafeDestinationType(t => typeof(IMessage).IsAssignableFrom(t));

            _registered = true;
        }
    }
}
