using System.Text.Json;
using Cheetah.Core.Events;
using Cheetah.Saga;
using Cheetah.Saga.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Cheetah.Saga.Integration.Tests;

[Collection("Saga")]
public class SagaLifecycleTests
{
    private readonly PostgresSagaFixture _fx;

    public SagaLifecycleTests(PostgresSagaFixture fx) => _fx = fx;

    private async Task ResetAsync()
    {
        await using var ctx = _fx.CreateDbContext();
        await ctx.Database.ExecuteSqlRawAsync("TRUNCATE \"SagaInstances\"");
    }

    private SagaOrchestrator BuildOrchestrator(IServiceProvider sp)
    {
        var registry = new SagaRegistry();
        registry.Register(typeof(OrderProcessingSaga));
        var repo = sp.GetRequiredService<ISagaRepository>();
        return new SagaOrchestrator(sp, registry, repo, NullLogger<SagaOrchestrator>.Instance);
    }

    [Fact]
    public async Task Полный_lifecycle_Created_Invoiced_Paid_сохраняется_в_БД()
    {
        await ResetAsync();

        var orderId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();

        // Каждый event — отдельный scope (имитация разных HTTP-запросов / handler invocations).
        await DispatchAsync(new OrderCreatedEvent(orderId));
        await DispatchAsync(new InvoiceCreatedEvent(orderId, invoiceId));
        await DispatchAsync(new PaymentSucceededEvent(orderId));

        await using var verify = _fx.CreateDbContext();
        var instance = verify.SagaInstances.Single(x => x.CorrelationKey == orderId.ToString());
        instance.Status.ShouldBe(SagaStatus.Completed);

        using var doc = JsonDocument.Parse(instance.DataJson);
        doc.RootElement.GetProperty("orderId").GetGuid().ShouldBe(orderId);
        doc.RootElement.GetProperty("invoiceId").GetGuid().ShouldBe(invoiceId);
        doc.RootElement.GetProperty("paymentDone").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public async Task Уникальный_index_не_даёт_создать_дубль_по_correlation()
    {
        await ResetAsync();

        var orderId = Guid.NewGuid();
        await DispatchAsync(new OrderCreatedEvent(orderId));

        // Второй OrderCreated с тем же orderId — orchestrator найдёт существующую,
        // и не создаст вторую.
        await DispatchAsync(new OrderCreatedEvent(orderId));

        await using var verify = _fx.CreateDbContext();
        verify.SagaInstances.Count(x => x.CorrelationKey == orderId.ToString()).ShouldBe(1);
    }

    [Fact]
    public async Task Version_инкрементируется_на_каждом_update()
    {
        await ResetAsync();
        var orderId = Guid.NewGuid();

        await DispatchAsync(new OrderCreatedEvent(orderId));
        await DispatchAsync(new InvoiceCreatedEvent(orderId, Guid.NewGuid()));

        await using var verify = _fx.CreateDbContext();
        var instance = verify.SagaInstances.Single(x => x.CorrelationKey == orderId.ToString());
        instance.Version.ShouldBeGreaterThanOrEqualTo(1);
    }

    private async Task DispatchAsync(IEvent e)
    {
        var services = new ServiceCollection();
        services.AddSingleton(_fx);
        services.AddScoped(sp => sp.GetRequiredService<PostgresSagaFixture>().CreateDbContext());
        services.AddScoped<ISagaRepository, EfSagaRepository<SagaTestDbContext>>();
        services.AddTransient<OrderProcessingSaga>();
        await using var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();
        var orchestrator = BuildOrchestrator(scope.ServiceProvider);
        await orchestrator.HandleAsync(e);
    }
}

