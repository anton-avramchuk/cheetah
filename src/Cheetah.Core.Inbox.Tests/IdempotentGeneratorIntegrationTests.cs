using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace Cheetah.Core.Inbox.Tests;

/// <summary>
/// Проверяет, что Source Generator корректно обрабатывает [Idempotent]:
/// генерирует RegisterServices, который оборачивает хендлер в InboxIdempotentEventHandler.
/// </summary>
[Idempotent]
[Export(LifetimeType.Scoped, typeof(IEventHandler<TestEvent>))]
public class IdempotentTestHandler : IEventHandler<TestEvent>
{
    public static int CallCount;

    public ValueTask HandleAsync(TestEvent @event, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref CallCount);
        return ValueTask.CompletedTask;
    }
}

[DependsOn(typeof(CrmInboxModule))]
public partial class TestIdempotentModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
        => RegisterServices(context.Services);
}

public class IdempotentGeneratorIntegrationTests
{
    [Fact]
    public void Generated_RegisterServices_Wraps_Handler_In_InboxIdempotentEventHandler()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IInboxStore>(s =>
            s.AlreadyProcessedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()) ==
            ValueTask.FromResult(false)));
        services.AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(NullLogger<>));

        var module = new TestIdempotentModule();
        module.RegisterServices(services);

        var sp = services.BuildServiceProvider();
        var resolved = sp.GetRequiredService<IEventHandler<TestEvent>>();

        resolved.ShouldBeOfType<InboxIdempotentEventHandler<TestEvent>>();
    }

    [Fact]
    public async Task Handler_Is_Called_On_First_Event_And_Skipped_On_Duplicate()
    {
        var seen = new HashSet<Guid>();
        var inboxMock = new Mock<IInboxStore>();
        inboxMock.Setup(s => s.AlreadyProcessedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<Guid, string, CancellationToken>((id, _, _) => ValueTask.FromResult(seen.Contains(id)));
        inboxMock.Setup(s => s.AddAsync(It.IsAny<InboxMessage>(), It.IsAny<CancellationToken>()))
            .Returns<InboxMessage, CancellationToken>((m, _) => { seen.Add(m.EventId); return ValueTask.CompletedTask; });

        var services = new ServiceCollection();
        services.AddSingleton(inboxMock.Object);
        services.AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(NullLogger<>));
        new TestIdempotentModule().RegisterServices(services);

        var sp = services.BuildServiceProvider();
        var handler = sp.GetRequiredService<IEventHandler<TestEvent>>();

        IdempotentTestHandler.CallCount = 0;
        var @event = new TestEvent("once");

        await handler.HandleAsync(@event);
        await handler.HandleAsync(@event); // дубликат

        IdempotentTestHandler.CallCount.ShouldBe(1);
    }
}
