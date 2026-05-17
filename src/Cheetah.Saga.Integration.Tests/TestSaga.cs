using Cheetah.Core.Events;
using Cheetah.Saga;

namespace Cheetah.Saga.Integration.Tests;

public sealed record OrderCreatedEvent(Guid OrderId) : EventBase;
public sealed record InvoiceCreatedEvent(Guid OrderId, Guid InvoiceId) : EventBase;
public sealed record PaymentSucceededEvent(Guid OrderId) : EventBase;

public class OrderSagaData
{
    public Guid OrderId { get; set; }
    public Guid? InvoiceId { get; set; }
    public bool PaymentDone { get; set; }
}

[SagaStartedBy(typeof(OrderCreatedEvent))]
[SagaHandles(typeof(InvoiceCreatedEvent))]
[SagaHandles(typeof(PaymentSucceededEvent))]
public class OrderProcessingSaga : Saga<OrderSagaData>
{
    public override string GetCorrelation(IEvent @event) => @event switch
    {
        OrderCreatedEvent e => e.OrderId.ToString(),
        InvoiceCreatedEvent e => e.OrderId.ToString(),
        PaymentSucceededEvent e => e.OrderId.ToString(),
        _ => throw new InvalidOperationException()
    };

    public ValueTask On(OrderCreatedEvent e, CancellationToken ct)
    {
        Data.OrderId = e.OrderId;
        return ValueTask.CompletedTask;
    }

    public ValueTask On(InvoiceCreatedEvent e, CancellationToken ct)
    {
        Data.InvoiceId = e.InvoiceId;
        return ValueTask.CompletedTask;
    }

    public ValueTask On(PaymentSucceededEvent e, CancellationToken ct)
    {
        Data.PaymentDone = true;
        Complete();
        return ValueTask.CompletedTask;
    }
}
