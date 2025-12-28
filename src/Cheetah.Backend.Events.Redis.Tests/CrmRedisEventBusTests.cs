using Cheetah.Backend.Events.Redis;
using Cheetah.Backend.Redis;
using Cheetah.Core.Events;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;

namespace Cheetah.Backend.Events.Redis.Tests;

public class CrmRedisEventBusTests
{
    private readonly Mock<IRedisEventBus> _redisEventBusMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<IServiceProvider> _scopedServiceProviderMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private readonly CrmRedisEventBus _eventBus;
    private readonly RedisEventBusOptions _options;

    public CrmRedisEventBusTests()
    {
        _redisEventBusMock = new Mock<IRedisEventBus>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _scopedServiceProviderMock = new Mock<IServiceProvider>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();

        _options = new RedisEventBusOptions
        {
            InstanceName = "test-instance",
            ChannelPrefix = "test-events:"
        };

        var optionsMock = Options.Create(_options);

        _serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_scopedServiceProviderMock.Object);
        _serviceScopeFactoryMock.Setup(x => x.CreateScope()).Returns(_serviceScopeMock.Object);
        _serviceProviderMock.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(_serviceScopeFactoryMock.Object);

        _eventBus = new CrmRedisEventBus(_redisEventBusMock.Object, _serviceProviderMock.Object, optionsMock);
    }

    [Fact]
    public async Task PublishAsync_ShouldPublishEventToRedis()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Test message" };
        _redisEventBusMock
            .Setup(x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<TestEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _eventBus.PublishAsync(testEvent);

        // Assert
        _redisEventBusMock.Verify(x => x.PublishAsync(
            "test-events:TestEvent",
            testEvent,
            "test-instance",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishManyAsync_ShouldPublishMultipleEvents()
    {
        // Arrange
        var events = new[]
        {
            new TestEvent { Message = "Message 1" },
            new TestEvent { Message = "Message 2" },
            new TestEvent { Message = "Message 3" }
        };

        _redisEventBusMock
            .Setup(x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<TestEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _eventBus.PublishManyAsync(events);

        // Assert
        _redisEventBusMock.Verify(x => x.PublishAsync(
            "test-events:TestEvent",
            It.IsAny<TestEvent>(),
            "test-instance",
            It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public void Subscribe_ShouldSubscribeToRedisChannel()
    {
        // Arrange
        _redisEventBusMock
            .Setup(x => x.SubscribeAsync<TestEvent>(
                It.IsAny<string>(),
                It.IsAny<Func<TestEvent, Task>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        _eventBus.Subscribe<TestEvent, TestEventHandler>();

        // Assert
        _redisEventBusMock.Verify(x => x.SubscribeAsync<TestEvent>(
            "test-events:TestEvent",
            It.IsAny<Func<TestEvent, Task>>(),
            "test-instance",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Subscribe_CalledTwiceForSameEvent_ShouldSubscribeOnlyOnce()
    {
        // Arrange
        _redisEventBusMock
            .Setup(x => x.SubscribeAsync<TestEvent>(
                It.IsAny<string>(),
                It.IsAny<Func<TestEvent, Task>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        _eventBus.Subscribe<TestEvent, TestEventHandler>();
        _eventBus.Subscribe<TestEvent, TestEventHandler>();

        // Assert
        _redisEventBusMock.Verify(x => x.SubscribeAsync<TestEvent>(
            "test-events:TestEvent",
            It.IsAny<Func<TestEvent, Task>>(),
            "test-instance",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Subscribe_WithMultipleHandlers_ShouldRegisterBothHandlers()
    {
        // Arrange
        _redisEventBusMock
            .Setup(x => x.SubscribeAsync<TestEvent>(
                It.IsAny<string>(),
                It.IsAny<Func<TestEvent, Task>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        _eventBus.Subscribe<TestEvent, TestEventHandler>();
        _eventBus.Subscribe<TestEvent, AnotherTestEventHandler>();

        // Assert - Should only subscribe to Redis once
        _redisEventBusMock.Verify(x => x.SubscribeAsync<TestEvent>(
            "test-events:TestEvent",
            It.IsAny<Func<TestEvent, Task>>(),
            "test-instance",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleEvent_ShouldInvokeRegisteredHandler()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Test" };
        var handlerMock = new Mock<TestEventHandler>();
        handlerMock
            .Setup(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _scopedServiceProviderMock
            .Setup(x => x.GetService(typeof(TestEventHandler)))
            .Returns(handlerMock.Object);

        Func<TestEvent, Task>? capturedHandler = null;
        _redisEventBusMock
            .Setup(x => x.SubscribeAsync<TestEvent>(
                It.IsAny<string>(),
                It.IsAny<Func<TestEvent, Task>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Func<TestEvent, Task>, string, CancellationToken>((ch, handler, inst, ct) =>
            {
                capturedHandler = handler;
            })
            .Returns(Task.CompletedTask);

        _eventBus.Subscribe<TestEvent, TestEventHandler>();

        // Act
        if (capturedHandler != null)
        {
            await capturedHandler(testEvent);
        }

        // Assert
        handlerMock.Verify(x => x.HandleAsync(
            It.Is<TestEvent>(e => e.Message == "Test"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleEvent_WithMultipleHandlers_ShouldInvokeBothHandlers()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Test" };
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

        Func<TestEvent, Task>? capturedHandler = null;
        _redisEventBusMock
            .Setup(x => x.SubscribeAsync<TestEvent>(
                It.IsAny<string>(),
                It.IsAny<Func<TestEvent, Task>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Func<TestEvent, Task>, string, CancellationToken>((ch, handler, inst, ct) =>
            {
                capturedHandler = handler;
            })
            .Returns(Task.CompletedTask);

        _eventBus.Subscribe<TestEvent, TestEventHandler>();
        _eventBus.Subscribe<TestEvent, AnotherTestEventHandler>();

        // Act
        if (capturedHandler != null)
        {
            await capturedHandler(testEvent);
        }

        // Assert
        handler1Mock.Verify(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        handler2Mock.Verify(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleEvent_WhenHandlerThrows_ShouldContinueProcessingOtherHandlers()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Test" };
        var handler1Mock = new Mock<TestEventHandler>();
        var handler2Mock = new Mock<AnotherTestEventHandler>();

        handler1Mock
            .Setup(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Handler 1 failed"));

        handler2Mock
            .Setup(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _scopedServiceProviderMock
            .Setup(x => x.GetService(typeof(TestEventHandler)))
            .Returns(handler1Mock.Object);

        _scopedServiceProviderMock
            .Setup(x => x.GetService(typeof(AnotherTestEventHandler)))
            .Returns(handler2Mock.Object);

        Func<TestEvent, Task>? capturedHandler = null;
        _redisEventBusMock
            .Setup(x => x.SubscribeAsync<TestEvent>(
                It.IsAny<string>(),
                It.IsAny<Func<TestEvent, Task>>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Func<TestEvent, Task>, string, CancellationToken>((ch, handler, inst, ct) =>
            {
                capturedHandler = handler;
            })
            .Returns(Task.CompletedTask);

        _eventBus.Subscribe<TestEvent, TestEventHandler>();
        _eventBus.Subscribe<TestEvent, AnotherTestEventHandler>();

        // Act
        if (capturedHandler != null)
        {
            await capturedHandler(testEvent);
        }

        // Assert - Second handler should still be called
        handler2Mock.Verify(x => x.HandleAsync(It.IsAny<TestEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    public record TestEvent : EventBase
    {
        public string Message { get; set; } = string.Empty;
    }

    public class TestEventHandler : IEventHandler<TestEvent>
    {
        public virtual ValueTask HandleAsync(TestEvent @event, CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }
    }

    public class AnotherTestEventHandler : IEventHandler<TestEvent>
    {
        public virtual ValueTask HandleAsync(TestEvent @event, CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }
    }
}
