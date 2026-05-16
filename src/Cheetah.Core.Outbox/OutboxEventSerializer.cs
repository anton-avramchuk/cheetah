using System.Text.Json;
using Cheetah.Core.Events;

namespace Cheetah.Core.Outbox;

/// <summary>
/// Сериализация/десериализация событий для outbox. Использует AssemblyQualifiedName типа.
/// </summary>
public static class OutboxEventSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static OutboxMessage Serialize<TEvent>(TEvent @event) where TEvent : IEvent
    {
        var type = @event.GetType();
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            EventType = type.AssemblyQualifiedName
                        ?? throw new InvalidOperationException($"Event type {type.FullName} has no AssemblyQualifiedName"),
            Payload = JsonSerializer.Serialize(@event, type, Options),
            OccurredAt = @event.OccurredAt
        };
    }

    public static (Type Type, IEvent Event) Deserialize(OutboxMessage message)
    {
        var type = Type.GetType(message.EventType, throwOnError: true)
                   ?? throw new InvalidOperationException($"Cannot resolve type '{message.EventType}'");
        var @event = (IEvent)(JsonSerializer.Deserialize(message.Payload, type, Options)
                              ?? throw new InvalidOperationException($"Cannot deserialize payload for {message.EventType}"));
        return (type, @event);
    }
}
