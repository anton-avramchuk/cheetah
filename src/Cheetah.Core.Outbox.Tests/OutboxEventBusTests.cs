using Cheetah.Core.Events;
using Cheetah.Core.Outbox;
using Moq;
using Shouldly;

namespace Cheetah.Core.Outbox.Tests;

public record TestEvent(string Name) : EventBase;

public class OutboxEventBusTests
{
    [Fact]
    public async Task PublishAsync_Saves_Message_To_Store_And_Does_Not_Call_Inner_Bus()
    {
        var store = new Mock<IOutboxStore>();
        var inner = new Mock<IInnerEventBus>();
        store.Setup(s => s.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var sut = new OutboxEventBus(store.Object, inner.Object);

        await sut.PublishAsync(new TestEvent("hi"));

        store.Verify(s => s.AddAsync(It.Is<OutboxMessage>(m =>
            m.EventType.Contains(nameof(TestEvent)) && m.Payload.Contains("hi")), It.IsAny<CancellationToken>()), Times.Once);
        inner.Verify(b => b.PublishAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PublishManyAsync_Adds_All_Messages()
    {
        var store = new Mock<IOutboxStore>();
        store.Setup(s => s.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        var sut = new OutboxEventBus(store.Object, Mock.Of<IInnerEventBus>());

        await sut.PublishManyAsync(new[] { new TestEvent("a"), new TestEvent("b"), new TestEvent("c") });

        store.Verify(s => s.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public void Subscribe_Delegates_To_Inner_Bus()
    {
        var inner = new Mock<IInnerEventBus>();
        var sut = new OutboxEventBus(Mock.Of<IOutboxStore>(), inner.Object);

        sut.Subscribe<TestEvent, TestHandler>();

        inner.Verify(b => b.Subscribe<TestEvent, TestHandler>(), Times.Once);
    }

    [Fact]
    public void Serializer_Serializes_And_Deserializes_Event()
    {
        var @event = new TestEvent("payload-x");
        var message = OutboxEventSerializer.Serialize(@event);
        message.EventType.ShouldContain(nameof(TestEvent));

        var (type, deserialized) = OutboxEventSerializer.Deserialize(message);
        type.ShouldBe(typeof(TestEvent));
        ((TestEvent)deserialized).Name.ShouldBe("payload-x");
        deserialized.EventId.ShouldBe(@event.EventId);
    }

    private class TestHandler : IEventHandler<TestEvent>
    {
        public ValueTask HandleAsync(TestEvent @event, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }
}
