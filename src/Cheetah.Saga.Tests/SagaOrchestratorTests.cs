using System.Text.Json;
using Cheetah.Saga;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Cheetah.Saga.Tests;

public class SagaOrchestratorTests
{
    private static (SagaOrchestrator orchestrator, InMemorySagaRepository repo) Build(params Type[] sagaTypes)
    {
        var services = new ServiceCollection();
        foreach (var t in sagaTypes) services.AddTransient(t);

        var sp = services.BuildServiceProvider();
        var registry = new SagaRegistry();
        foreach (var t in sagaTypes) registry.Register(t);

        var repo = new InMemorySagaRepository();
        var orchestrator = new SagaOrchestrator(sp, registry, repo, NullLogger<SagaOrchestrator>.Instance);
        return (orchestrator, repo);
    }

    [Fact]
    public async Task SagaStartedBy_создаёт_новую_instance_и_сохраняет_данные()
    {
        var (orch, repo) = Build(typeof(OrderProcessingSaga));

        var orderId = Guid.NewGuid();
        await orch.HandleAsync(new OrderCreatedEvent(orderId));

        var instance = repo.All.Single();
        instance.CorrelationKey.ShouldBe(orderId.ToString());
        instance.Status.ShouldBe(SagaStatus.Running);

        using var doc = JsonDocument.Parse(instance.DataJson);
        doc.RootElement.GetProperty("orderId").GetGuid().ShouldBe(orderId);
    }

    [Fact]
    public async Task Последовательные_события_продолжают_ту_же_сагу()
    {
        var (orch, repo) = Build(typeof(OrderProcessingSaga));
        var orderId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();

        await orch.HandleAsync(new OrderCreatedEvent(orderId));
        await orch.HandleAsync(new InvoiceCreatedEvent(orderId, invoiceId));

        var instance = repo.All.Single();
        using var doc = JsonDocument.Parse(instance.DataJson);
        doc.RootElement.GetProperty("invoiceId").GetGuid().ShouldBe(invoiceId);
    }

    [Fact]
    public async Task PaymentSucceeded_завершает_сагу_статусом_Completed()
    {
        var (orch, repo) = Build(typeof(OrderProcessingSaga));
        var orderId = Guid.NewGuid();

        await orch.HandleAsync(new OrderCreatedEvent(orderId));
        await orch.HandleAsync(new PaymentSucceededEvent(orderId));

        repo.All.Single().Status.ShouldBe(SagaStatus.Completed);
    }

    [Fact]
    public async Task PaymentFailed_переводит_сагу_в_Compensating_с_Reason()
    {
        var (orch, repo) = Build(typeof(OrderProcessingSaga));
        var orderId = Guid.NewGuid();

        await orch.HandleAsync(new OrderCreatedEvent(orderId));
        await orch.HandleAsync(new PaymentFailedEvent(orderId, "insufficient funds"));

        var instance = repo.All.Single();
        instance.Status.ShouldBe(SagaStatus.Compensating);
        instance.Reason.ShouldBe("insufficient funds");
    }

    [Fact]
    public async Task Event_без_активной_саги_и_не_StartedBy_игнорируется()
    {
        var (orch, repo) = Build(typeof(OrderProcessingSaga));
        await orch.HandleAsync(new InvoiceCreatedEvent(Guid.NewGuid(), Guid.NewGuid()));
        repo.All.ShouldBeEmpty();
    }

    [Fact]
    public async Task Завершённая_сага_не_реагирует_на_новые_события()
    {
        var (orch, repo) = Build(typeof(OrderProcessingSaga));
        var orderId = Guid.NewGuid();

        await orch.HandleAsync(new OrderCreatedEvent(orderId));
        await orch.HandleAsync(new PaymentSucceededEvent(orderId));

        // Повторное PaymentFailed уже после Completed — должно проигнориться.
        await orch.HandleAsync(new PaymentFailedEvent(orderId, "late"));

        var instance = repo.All.Single();
        instance.Status.ShouldBe(SagaStatus.Completed);
        instance.Reason.ShouldBeNull();
    }

    [Fact]
    public async Task Исключение_в_handler_переводит_сагу_в_Failed_с_Reason()
    {
        var (orch, repo) = Build(typeof(FailingSaga));
        var orderId = Guid.NewGuid();

        await orch.HandleAsync(new OrderCreatedEvent(orderId));

        var instance = repo.All.Single();
        instance.Status.ShouldBe(SagaStatus.Failed);
        instance.Reason.ShouldBe("boom");
    }

    [Fact]
    public async Task Несколько_типов_саг_могут_реагировать_на_один_event_независимо()
    {
        var (orch, repo) = Build(typeof(OrderProcessingSaga), typeof(FailingSaga));
        var orderId = Guid.NewGuid();

        await orch.HandleAsync(new OrderCreatedEvent(orderId));

        repo.All.Count.ShouldBe(2);
        repo.All.Single(x => x.SagaType.Contains(nameof(OrderProcessingSaga))).Status.ShouldBe(SagaStatus.Running);
        repo.All.Single(x => x.SagaType.Contains(nameof(FailingSaga))).Status.ShouldBe(SagaStatus.Failed);
    }
}
