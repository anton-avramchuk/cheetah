using Cheetah.Core.Events;

namespace Cheetah.Modules.SalesDocuments.DomainEvents;

/// <summary>Коммерческий документ создан (черновик).</summary>
public record DocumentCreatedIntegrationEvent(Guid DocumentId, int DocType, Guid CustomerId, Guid OwnerId) : EventBase;

/// <summary>Документ выпущен/отправлен (КП — Sent, заказ — Confirmed, счёт — Issued). Потребитель — Notification.</summary>
public record DocumentSentIntegrationEvent(
    Guid DocumentId, int DocType, string Number, Guid CustomerId, decimal GrandTotal, string Currency) : EventBase;

/// <summary>КП принято клиентом. Потребитель — Deals (двигать сделку в Won).</summary>
public record QuoteAcceptedIntegrationEvent(
    Guid DocumentId, Guid? DealId, Guid CustomerId, decimal GrandTotal, string Currency) : EventBase;

/// <summary>КП отклонено клиентом.</summary>
public record QuoteRejectedIntegrationEvent(Guid DocumentId, string? Reason) : EventBase;

/// <summary>Счёт оплачен.</summary>
public record InvoicePaidIntegrationEvent(Guid DocumentId, Guid CustomerId, decimal GrandTotal, string Currency) : EventBase;

/// <summary>Счёт просрочен (фоновый скан). Потребитель — Notification (напоминание).</summary>
public record InvoiceOverdueIntegrationEvent(Guid DocumentId, Guid CustomerId, decimal GrandTotal, string Currency) : EventBase;

/// <summary>Документ аннулирован.</summary>
public record DocumentCancelledIntegrationEvent(Guid DocumentId, string? Reason) : EventBase;
