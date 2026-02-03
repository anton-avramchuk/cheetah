using Cheetah.Backend.Redis;
using Shouldly;
using Moq;
using StackExchange.Redis;
using System.Text.Json;

namespace Cheetah.Backend.Redis.Tests;

public class RedisEventBusTests
{
    private readonly Mock<IRedisConnectionProvider> _connectionProviderMock;
    private readonly Mock<IConnectionMultiplexer> _connectionMock;
    private readonly Mock<ISubscriber> _subscriberMock;
    private readonly RedisEventBus _eventBus;

    public RedisEventBusTests()
    {
        _connectionProviderMock = new Mock<IRedisConnectionProvider>();
        _connectionMock = new Mock<IConnectionMultiplexer>();
        _subscriberMock = new Mock<ISubscriber>();

        _connectionMock
            .Setup(x => x.GetSubscriber(It.IsAny<object>()))
            .Returns(_subscriberMock.Object);

        _connectionProviderMock
            .Setup(x => x.GetConnection(It.IsAny<string>()))
            .Returns(_connectionMock.Object);

        _eventBus = new RedisEventBus(_connectionProviderMock.Object);
    }

    [Fact]
    public async Task PublishAsync_ShouldSerializeAndPublishEvent()
    {
        // Arrange
        var testEvent = new TestEvent { Id = 1, Message = "Test" };
        _subscriberMock
            .Setup(x => x.PublishAsync(
                It.IsAny<RedisChannel>(),
                It.IsAny<RedisValue>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(0);

        // Act
        await _eventBus.PublishAsync("test-channel", testEvent);

        // Assert
        _subscriberMock.Verify(x => x.PublishAsync(
            It.Is<RedisChannel>(c => c.ToString() == "test-channel"),
            It.Is<RedisValue>(v => v.ToString().Contains("Test")),
            It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_WithCustomInstance_ShouldUseCorrectInstance()
    {
        // Arrange
        var testEvent = new TestEvent { Id = 1, Message = "Test" };
        _subscriberMock
            .Setup(x => x.PublishAsync(
                It.IsAny<RedisChannel>(),
                It.IsAny<RedisValue>(),
                It.IsAny<CommandFlags>()))
            .ReturnsAsync(0);

        // Act
        await _eventBus.PublishAsync("test-channel", testEvent, "events");

        // Assert
        _connectionProviderMock.Verify(x => x.GetConnection("events"), Times.Once);
    }

    [Fact]
    public async Task SubscribeAsync_ShouldRegisterHandler()
    {
        // Arrange
        Task Handler(TestEvent e) => Task.CompletedTask;

        _subscriberMock
            .Setup(x => x.SubscribeAsync(
                It.IsAny<RedisChannel>(),
                It.IsAny<Action<RedisChannel, RedisValue>>(),
                It.IsAny<CommandFlags>()))
            .Returns(Task.CompletedTask);

        // Act
        await _eventBus.SubscribeAsync<TestEvent>("test-channel", Handler);

        // Assert
        _subscriberMock.Verify(x => x.SubscribeAsync(
            It.Is<RedisChannel>(c => c.ToString() == "test-channel"),
            It.IsAny<Action<RedisChannel, RedisValue>>(),
            It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task SubscribeAsync_WhenEventReceived_ShouldInvokeHandler()
    {
        // Arrange
        var receivedEvent = default(TestEvent);
        var tcs = new TaskCompletionSource<bool>();

        Task Handler(TestEvent e)
        {
            receivedEvent = e;
            tcs.SetResult(true);
            return Task.CompletedTask;
        }

        Action<RedisChannel, RedisValue>? capturedHandler = null;
        _subscriberMock
            .Setup(x => x.SubscribeAsync(
                It.IsAny<RedisChannel>(),
                It.IsAny<Action<RedisChannel, RedisValue>>(),
                It.IsAny<CommandFlags>()))
            .Callback<RedisChannel, Action<RedisChannel, RedisValue>, CommandFlags>((ch, handler, flags) =>
            {
                capturedHandler = handler;
            })
            .Returns(Task.CompletedTask);

        await _eventBus.SubscribeAsync<TestEvent>("test-channel", Handler);

        // Act
        var testEvent = new TestEvent { Id = 42, Message = "Hello" };
        var serialized = JsonSerializer.Serialize(testEvent);
        capturedHandler?.Invoke(RedisChannel.Literal("test-channel"), serialized);

        // Wait for handler to be called
        await Task.WhenAny(tcs.Task, Task.Delay(1000));

        // Assert
        receivedEvent.ShouldNotBeNull();
        receivedEvent!.Id.ShouldBe(42);
        receivedEvent.Message.ShouldBe("Hello");
    }

    [Fact]
    public async Task UnsubscribeAsync_ShouldRemoveHandlers()
    {
        // Arrange
        Action<RedisChannel, RedisValue>? capturedHandler = null;
        _subscriberMock
            .Setup(x => x.SubscribeAsync(
                It.IsAny<RedisChannel>(),
                It.IsAny<Action<RedisChannel, RedisValue>>(),
                It.IsAny<CommandFlags>()))
            .Callback<RedisChannel, Action<RedisChannel, RedisValue>, CommandFlags>((ch, handler, flags) =>
            {
                capturedHandler = handler;
            })
            .Returns(Task.CompletedTask);

        _subscriberMock
            .Setup(x => x.UnsubscribeAsync(
                It.IsAny<RedisChannel>(),
                It.IsAny<Action<RedisChannel, RedisValue>>(),
                It.IsAny<CommandFlags>()))
            .Returns(Task.CompletedTask);

        await _eventBus.SubscribeAsync<TestEvent>("test-channel", _ => Task.CompletedTask);

        // Act
        await _eventBus.UnsubscribeAsync("test-channel");

        // Assert
        _subscriberMock.Verify(x => x.UnsubscribeAsync(
            It.Is<RedisChannel>(c => c.ToString() == "test-channel"),
            It.IsAny<Action<RedisChannel, RedisValue>>(),
            It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task SubscribeAsync_MultipleHandlers_ShouldRegisterAll()
    {
        // Arrange
        Task Handler1(TestEvent e) => Task.CompletedTask;
        Task Handler2(TestEvent e) => Task.CompletedTask;

        _subscriberMock
            .Setup(x => x.SubscribeAsync(
                It.IsAny<RedisChannel>(),
                It.IsAny<Action<RedisChannel, RedisValue>>(),
                It.IsAny<CommandFlags>()))
            .Returns(Task.CompletedTask);

        // Act
        await _eventBus.SubscribeAsync<TestEvent>("test-channel", Handler1);
        await _eventBus.SubscribeAsync<TestEvent>("test-channel", Handler2);

        // Assert
        _subscriberMock.Verify(x => x.SubscribeAsync(
            It.Is<RedisChannel>(c => c.ToString() == "test-channel"),
            It.IsAny<Action<RedisChannel, RedisValue>>(),
            It.IsAny<CommandFlags>()), Times.Exactly(2));
    }

    [Fact]
    public async Task UnsubscribeAsync_WithNonExistentChannel_ShouldNotThrow()
    {
        // Act
        var act = async () => await _eventBus.UnsubscribeAsync("nonexistent-channel");

        // Assert
        await Should.NotThrowAsync(act);
    }

    [Fact]
    public async Task SubscribeAsync_WithDifferentInstances_ShouldUseSeparateChannels()
    {
        // Arrange
        Task Handler(TestEvent e) => Task.CompletedTask;

        _subscriberMock
            .Setup(x => x.SubscribeAsync(
                It.IsAny<RedisChannel>(),
                It.IsAny<Action<RedisChannel, RedisValue>>(),
                It.IsAny<CommandFlags>()))
            .Returns(Task.CompletedTask);

        // Act
        await _eventBus.SubscribeAsync<TestEvent>("test-channel", Handler, "default");
        await _eventBus.SubscribeAsync<TestEvent>("test-channel", Handler, "events");

        // Assert
        _connectionProviderMock.Verify(x => x.GetConnection("default"), Times.Once);
        _connectionProviderMock.Verify(x => x.GetConnection("events"), Times.Once);
    }

    private class TestEvent
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
