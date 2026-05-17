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
    public async Task SagaStartedBy_Creates_New_Instance_And_Saves_Data()
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
    public async Task Sequential_Events_Continue_Same_Saga()
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
    public async Task PaymentSucceeded_Finishes_Saga_With_Completed_Status()
    {
        var (orch, repo) = Build(typeof(OrderProcessingSaga));
        var orderId = Guid.NewGuid();

        await orch.HandleAsync(new OrderCreatedEvent(orderId));
        await orch.HandleAsync(new PaymentSucceededEvent(orderId));

        repo.All.Single().Status.ShouldBe(SagaStatus.Completed);
    }

    [Fact]
    public async Task PaymentFailed_Moves_Saga_To_Compensating_With_Reason()
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
    public async Task Event_Without_Active_Saga_And_Not_StartedBy_Is_Ignored()
    {
        var (orch, repo) = Build(typeof(OrderProcessingSaga));
        await orch.HandleAsync(new InvoiceCreatedEvent(Guid.NewGuid(), Guid.NewGuid()));
        repo.All.ShouldBeEmpty();
    }

    [Fact]
    public async Task Completed_Saga_Does_Not_React_To_New_Events()
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
    public async Task Exception_In_Handler_Moves_Saga_To_Failed_With_Reason()
    {
        var (orch, repo) = Build(typeof(FailingSaga));
        var orderId = Guid.NewGuid();

        await orch.HandleAsync(new OrderCreatedEvent(orderId));

        var instance = repo.All.Single();
        instance.Status.ShouldBe(SagaStatus.Failed);
        instance.Reason.ShouldBe("boom");
    }

    [Fact]
    public async Task Multiple_Saga_Types_Can_React_To_One_Event_Independently()
    {
        var (orch, repo) = Build(typeof(OrderProcessingSaga), typeof(FailingSaga));
        var orderId = Guid.NewGuid();

        await orch.HandleAsync(new OrderCreatedEvent(orderId));

        repo.All.Count.ShouldBe(2);
        repo.All.Single(x => x.SagaType.Contains(nameof(OrderProcessingSaga))).Status.ShouldBe(SagaStatus.Running);
        repo.All.Single(x => x.SagaType.Contains(nameof(FailingSaga))).Status.ShouldBe(SagaStatus.Failed);
    }
}
