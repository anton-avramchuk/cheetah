using Cheetah.Core.Events;
using Cheetah.Frontend.Events;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Frontend.Events.Tests;

public class CrmInMemoryEventBusTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly CrmInMemoryEventBus _eventBus;

    public CrmInMemoryEventBusTests()
    {
        var services = new ServiceCollection();

        // Register test handlers
        services.AddScoped<TestEventHandler>();
        services.AddScoped<AnotherTestEventHandler>();

        _serviceProvider = services.BuildServiceProvider();
        _eventBus = new CrmInMemoryEventBus(_serviceProvider);
    }

    [Fact]
    public async Task PublishAsync_WithNoSubscribers_ShouldNotThrow()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Test message" };

        // Act
        var act = async () => await _eventBus.PublishAsync(testEvent);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_WithSubscriber_ShouldInvokeHandler()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Test" };
        TestEventHandler.InvokedEvents.Clear();

        _eventBus.Subscribe<TestEvent, TestEventHandler>();

        // Act
        await _eventBus.PublishAsync(testEvent);

        // Assert
        TestEventHandler.InvokedEvents.Should().ContainSingle();
        TestEventHandler.InvokedEvents[0].Message.Should().Be("Test");
    }

    [Fact]
    public async Task PublishAsync_WithMultipleSubscribers_ShouldInvokeAllHandlers()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Test" };
        TestEventHandler.InvokedEvents.Clear();
        AnotherTestEventHandler.InvokedEvents.Clear();

        _eventBus.Subscribe<TestEvent, TestEventHandler>();
        _eventBus.Subscribe<TestEvent, AnotherTestEventHandler>();

        // Act
        await _eventBus.PublishAsync(testEvent);

        // Assert
        TestEventHandler.InvokedEvents.Should().ContainSingle();
        AnotherTestEventHandler.InvokedEvents.Should().ContainSingle();
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
        TestEventHandler.InvokedEvents.Clear();

        _eventBus.Subscribe<TestEvent, TestEventHandler>();

        // Act
        await _eventBus.PublishManyAsync(events);

        // Assert
        TestEventHandler.InvokedEvents.Should().HaveCount(3);
        TestEventHandler.InvokedEvents[0].Message.Should().Be("Message 1");
        TestEventHandler.InvokedEvents[1].Message.Should().Be("Message 2");
        TestEventHandler.InvokedEvents[2].Message.Should().Be("Message 3");
    }

    [Fact]
    public async Task Subscribe_CalledMultipleTimes_ShouldRegisterOnlyOnce()
    {
        // Arrange
        TestEventHandler.InvokedEvents.Clear();
        var testEvent = new TestEvent { Message = "Test" };

        // Act
        _eventBus.Subscribe<TestEvent, TestEventHandler>();
        _eventBus.Subscribe<TestEvent, TestEventHandler>();
        _eventBus.Subscribe<TestEvent, TestEventHandler>();

        // Assert - Handler should be invoked only once per event
        await _eventBus.PublishAsync(testEvent);
        TestEventHandler.InvokedEvents.Should().ContainSingle();
    }

    [Fact]
    public async Task PublishAsync_WhenHandlerThrows_ShouldContinueProcessingOtherHandlers()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Test" };
        TestEventHandler.ShouldThrow = true;
        TestEventHandler.InvokedEvents.Clear();
        AnotherTestEventHandler.InvokedEvents.Clear();

        _eventBus.Subscribe<TestEvent, TestEventHandler>();
        _eventBus.Subscribe<TestEvent, AnotherTestEventHandler>();

        // Act
        await _eventBus.PublishAsync(testEvent);

        // Assert - Second handler should still be called despite first handler throwing
        AnotherTestEventHandler.InvokedEvents.Should().ContainSingle();
        TestEventHandler.ShouldThrow = false; // Reset for other tests
    }

    [Fact]
    public async Task PublishAsync_ShouldCreateNewScopeForHandlers()
    {
        // Arrange
        var testEvent = new TestEvent { Message = "Test" };
        TestEventHandler.InvokedEvents.Clear();

        _eventBus.Subscribe<TestEvent, TestEventHandler>();

        // Act
        await _eventBus.PublishAsync(testEvent);
        await _eventBus.PublishAsync(testEvent);

        // Assert - Each publish should create a new scope and invoke handler
        TestEventHandler.InvokedEvents.Should().HaveCount(2);
    }

    [Fact]
    public async Task PublishManyAsync_WithNoSubscribers_ShouldNotThrow()
    {
        // Arrange
        var events = new[]
        {
            new TestEvent { Message = "Message 1" },
            new TestEvent { Message = "Message 2" }
        };

        // Act
        var act = async () => await _eventBus.PublishManyAsync(events);

        // Assert
        await act.Should().NotThrowAsync();
    }

    // Test event and handlers
    public record TestEvent : EventBase
    {
        public string Message { get; set; } = string.Empty;
    }

    public class TestEventHandler : IEventHandler<TestEvent>
    {
        public static List<TestEvent> InvokedEvents { get; } = new();
        public static bool ShouldThrow { get; set; }

        public ValueTask HandleAsync(TestEvent @event, CancellationToken cancellationToken = default)
        {
            if (ShouldThrow)
            {
                throw new Exception("Test exception");
            }

            InvokedEvents.Add(@event);
            return ValueTask.CompletedTask;
        }
    }

    public class AnotherTestEventHandler : IEventHandler<TestEvent>
    {
        public static List<TestEvent> InvokedEvents { get; } = new();

        public ValueTask HandleAsync(TestEvent @event, CancellationToken cancellationToken = default)
        {
            InvokedEvents.Add(@event);
            return ValueTask.CompletedTask;
        }
    }
}
