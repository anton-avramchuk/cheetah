using Cheetah.Backend.Events.InMemory;
using Cheetah.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;

namespace Cheetah.Backend.Events.InMemory.Tests;

public class InMemoryEventBusTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceProvider> _scopedServiceProviderMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly InMemoryEventBus _eventBus;

    public InMemoryEventBusTests()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _scopedServiceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_scopedServiceProviderMock.Object);
        _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
        _serviceProviderMock
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_serviceScopeFactoryMock.Object);

        _eventBus = new InMemoryEventBus(_serviceProviderMock.Object);
    }

    [Fact]
    public async Task PublishAsync_WithNoSubscribers_ShouldCompleteWithoutError()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Hello" };

        // Act & Assert
        await Should.NotThrowAsync(() => _eventBus.PublishAsync(testEvent).AsTask());
    }

    [Fact]
    public async Task PublishAsync_WithRegisteredHandler_ShouldInvokeHandler()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Hello" };
        var handlerMock = new Mock<TestEventHandler>();
        handlerMock
            .Setup(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _scopedServiceProviderMock
            .Setup(x => x.GetService(typeof(TestEventHandler)))
            .Returns(handlerMock.Object);

        _eventBus.Subscribe<TestEvent, TestEventHandler>();

        // Act
        await _eventBus.PublishAsync(testEvent);

        // Assert
        handlerMock.Verify(
            x => x.HandleAsync(It.Is<TestEvent>(e => e.Message == "Hello"), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldPassCancellationTokenToHandler()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Hello" };
        var cts = new CancellationTokenSource();
        var capturedToken = CancellationToken.None;

        var handlerMock = new Mock<TestEventHandler>();
        handlerMock
            .Setup(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .Callback<TestEvent, CancellationToken>((_, ct) => capturedToken = ct)
            .Returns(ValueTask.CompletedTask);

        _scopedServiceProviderMock
            .Setup(x => x.GetService(typeof(TestEventHandler)))
            .Returns(handlerMock.Object);

        _eventBus.Subscribe<TestEvent, TestEventHandler>();

        // Act
        await _eventBus.PublishAsync(testEvent, cts.Token);

        // Assert
        capturedToken.ShouldBe(cts.Token);
    }

    [Fact]
    public async Task PublishAsync_WithMultipleHandlers_ShouldInvokeAllHandlers()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Hello" };
        var handler1Mock = new Mock<TestEventHandler>();
        var handler2Mock = new Mock<AnotherTestEventHandler>();

        handler1Mock
            .Setup(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        handler2Mock
            .Setup(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _scopedServiceProviderMock
            .Setup(x => x.GetService(typeof(TestEventHandler)))
            .Returns(handler1Mock.Object);
        _scopedServiceProviderMock
            .Setup(x => x.GetService(typeof(AnotherTestEventHandler)))
            .Returns(handler2Mock.Object);

        _eventBus.Subscribe<TestEvent, TestEventHandler>();
        _eventBus.Subscribe<TestEvent, AnotherTestEventHandler>();

        // Act
        await _eventBus.PublishAsync(testEvent);

        // Assert
        handler1Mock.Verify(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        handler2Mock.Verify(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WhenHandlerThrows_ShouldContinueProcessingOtherHandlers()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Hello" };
        var handler1Mock = new Mock<TestEventHandler>();
        var handler2Mock = new Mock<AnotherTestEventHandler>();

        handler1Mock
            .Setup(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Handler 1 failed"));
        handler2Mock
            .Setup(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _scopedServiceProviderMock
            .Setup(x => x.GetService(typeof(TestEventHandler)))
            .Returns(handler1Mock.Object);
        _scopedServiceProviderMock
            .Setup(x => x.GetService(typeof(AnotherTestEventHandler)))
            .Returns(handler2Mock.Object);

        _eventBus.Subscribe<TestEvent, TestEventHandler>();
        _eventBus.Subscribe<TestEvent, AnotherTestEventHandler>();

        // Act — should not throw
        await Should.NotThrowAsync(() => _eventBus.PublishAsync(testEvent).AsTask());

        // Assert — second handler still called
        handler2Mock.Verify(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WhenHandlerNotRegisteredInDI_ShouldNotThrow()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Hello" };

        _scopedServiceProviderMock
            .Setup(x => x.GetService(typeof(TestEventHandler)))
            .Returns((object?)null);

        _eventBus.Subscribe<TestEvent, TestEventHandler>();

        // Act & Assert
        await Should.NotThrowAsync(() => _eventBus.PublishAsync(testEvent).AsTask());
    }

    [Fact]
    public async Task PublishManyAsync_ShouldInvokeHandlerForEachEvent()
    {
        // Arrange
        var events = new[]
        {
            new TestEvent { Message = "Message 1" },
            new TestEvent { Message = "Message 2" },
            new TestEvent { Message = "Message 3" }
        };

        var handlerMock = new Mock<TestEventHandler>();
        handlerMock
            .Setup(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _scopedServiceProviderMock
            .Setup(x => x.GetService(typeof(TestEventHandler)))
            .Returns(handlerMock.Object);

        _eventBus.Subscribe<TestEvent, TestEventHandler>();

        // Act
        await _eventBus.PublishManyAsync(events);

        // Assert
        handlerMock.Verify(
            x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task PublishManyAsync_WithEmptyCollection_ShouldCompleteWithoutError()
    {
        // Act & Assert
        await Should.NotThrowAsync(() => _eventBus.PublishManyAsync(Array.Empty<TestEvent>()).AsTask());
    }

    [Fact]
    public void Subscribe_ShouldRegisterHandler()
    {
        // Act
        _eventBus.Subscribe<TestEvent, TestEventHandler>();

        // Assert — no exception, handler can be verified by publishing
        // (State is internal; verification is done via publish behavior)
    }

    [Fact]
    public async Task Subscribe_CalledTwiceWithSameHandler_ShouldInvokeHandlerOnce()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Hello" };
        var handlerMock = new Mock<TestEventHandler>();
        handlerMock
            .Setup(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _scopedServiceProviderMock
            .Setup(x => x.GetService(typeof(TestEventHandler)))
            .Returns(handlerMock.Object);

        // Register the same handler twice
        _eventBus.Subscribe<TestEvent, TestEventHandler>();
        _eventBus.Subscribe<TestEvent, TestEventHandler>();

        // Act
        await _eventBus.PublishAsync(testEvent);

        // Assert — handler should only be called once (deduplication)
        handlerMock.Verify(
            x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishAsync_ShouldCreateNewScopePerPublish()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Hello" };
        var handlerMock = new Mock<TestEventHandler>();
        handlerMock
            .Setup(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _scopedServiceProviderMock
            .Setup(x => x.GetService(typeof(TestEventHandler)))
            .Returns(handlerMock.Object);

        _eventBus.Subscribe<TestEvent, TestEventHandler>();

        // Act
        await _eventBus.PublishAsync(testEvent);
        await _eventBus.PublishAsync(testEvent);

        // Assert — a new scope is created for each publish
        _serviceScopeFactoryMock.Verify(x => x.CreateScope(), Times.Exactly(2));
    }

    [Fact]
    public async Task PublishAsync_HandlerForDifferentEventType_ShouldNotBeInvoked()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Hello" };
        var otherHandlerMock = new Mock<OtherEventHandler>();
        otherHandlerMock
            .Setup(x => x.HandleAsync(It.IsAny<OtherTestEvent>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _scopedServiceProviderMock
            .Setup(x => x.GetService(typeof(OtherEventHandler)))
            .Returns(otherHandlerMock.Object);

        _eventBus.Subscribe<OtherTestEvent, OtherEventHandler>();

        // Act
        await _eventBus.PublishAsync(testEvent);

        // Assert — handler for OtherTestEvent should NOT be called
        otherHandlerMock.Verify(
            x => x.HandleAsync(It.IsAny<OtherTestEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // Test fixtures

    public record TestEvent : EventBase
    {
        public string Message { get; set; } = string.Empty;
    }

    public record OtherTestEvent : EventBase
    {
        public int Value { get; set; }
    }

    public class TestEventHandler : IEventHandler<TestEvent>
    {
        public virtual ValueTask HandleAsync(TestEvent @event, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }

    public class AnotherTestEventHandler : IEventHandler<TestEvent>
    {
        public virtual ValueTask HandleAsync(TestEvent @event, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }

    public class OtherEventHandler : IEventHandler<OtherTestEvent>
    {
        public virtual ValueTask HandleAsync(OtherTestEvent @event, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }
}
