namespace Cheetah.Modules.SalesDocuments.Shared;

/// <summary>Тип коммерческого документа (дискриминатор единого агрегата).</summary>
public enum DocType
{
    Quote = 0,
    Order = 1,
    Invoice = 2
}

/// <summary>
/// Единый набор статусов всех типов документов. Допустимость перехода зависит от <see cref="DocType"/>
/// и проверяется доменными guard'ами + конечным автоматом (<c>IStateMachineValidator</c>).
/// </summary>
public enum DocumentStatus
{
    Draft = 0,

    // Quote
    Sent = 1,
    Accepted = 2,
    Rejected = 3,
    Expired = 4,

    // Order
    Confirmed = 5,
    Fulfilled = 6,

    // Invoice
    Issued = 7,
    Paid = 8,
    Overdue = 9,

    // общий терминальный
    Cancelled = 10
}
