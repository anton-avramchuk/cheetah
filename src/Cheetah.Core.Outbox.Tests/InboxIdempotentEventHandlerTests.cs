using Cheetah.Core.Events;
using Cheetah.Core.Outbox;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace Cheetah.Core.Outbox.Tests;

public class InboxIdempotentEventHandlerTests
{
    [Fact]
    public async Task При_первом_событии_зовётся_inner_и_пишется_inbox()
    {
        var inner = new Mock<IEventHandler<TestEvent>>();
        var inbox = new Mock<IInboxStore>();
        inbox.Setup(s => s.AlreadyProcessedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = new InboxIdempotentEventHandler<TestEvent>(inner.Object, inbox.Object,
            NullLogger<InboxIdempotentEventHandler<TestEvent>>.Instance);

        var @event = new TestEvent("hello");
        await sut.HandleAsync(@event);

        inner.Verify(h => h.HandleAsync(@event, It.IsAny<CancellationToken>()), Times.Once);
        inbox.Verify(s => s.AddAsync(It.Is<InboxMessage>(m =>
            m.EventId == @event.EventId &&
            m.ConsumerName == inner.Object.GetType().FullName), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Если_уже_обработано_inner_не_вызывается()
    {
        var inner = new Mock<IEventHandler<TestEvent>>();
        var inbox = new Mock<IInboxStore>();
        inbox.Setup(s => s.AlreadyProcessedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = new InboxIdempotentEventHandler<TestEvent>(inner.Object, inbox.Object,
            NullLogger<InboxIdempotentEventHandler<TestEvent>>.Instance);

        await sut.HandleAsync(new TestEvent("dup"));

        inner.Verify(h => h.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        inbox.Verify(s => s.AddAsync(It.IsAny<InboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Если_inner_бросает_inbox_не_пишется()
    {
        var inner = new Mock<IEventHandler<TestEvent>>();
        inner.Setup(h => h.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));
        var inbox = new Mock<IInboxStore>();
        inbox.Setup(s => s.AlreadyProcessedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = new InboxIdempotentEventHandler<TestEvent>(inner.Object, inbox.Object,
            NullLogger<InboxIdempotentEventHandler<TestEvent>>.Instance);

        await Should.ThrowAsync<InvalidOperationException>(() => sut.HandleAsync(new TestEvent("x")).AsTask());

        inbox.Verify(s => s.AddAsync(It.IsAny<InboxMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
