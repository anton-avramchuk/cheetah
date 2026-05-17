namespace Cheetah.Core.Events;

/// <summary>
/// Стандартные ключи для keyed-регистрации IEventBus. Используются для отправки в конкретный
/// транспорт через [FromKeyedServices(EventBusKeys.Kafka)] IEventBus.
///
/// Параллельно с keyed-регистрацией модуль транспорта обычно делает и дефолтную регистрацию
/// IEventBus — тогда хендлеры без указания ключа получают этот транспорт.
/// </summary>
public static class EventBusKeys
{
    public const string Redis = "redis";
    public const string Kafka = "kafka";
    public const string InMemory = "in-memory";
}
