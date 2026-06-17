using Cheetah.Contracts.Responses;
using Cheetah.Modules.SalesDocuments.Shared;

namespace Cheetah.Modules.SalesDocuments.Contracts;

/// <summary>
/// Базовый ViewModel коммерческого документа (граница API). Абстрактен: наследник объявляет конкретный
/// <c>sealed record SalesDocumentDto : SalesDocumentDtoBase</c> и при необходимости добавляет свои поля
/// (например, <c>PaymentTerms</c>, <c>ShipTo</c>). Это точка расширяемости ViewModel.
/// </summary>
public abstract record SalesDocumentDtoBase : ICrmResponse
{
    public Guid Id { get; init; }
    public DocType DocType { get; init; }
    public string Number { get; init; } = null!;
    public DocumentStatus Status { get; init; }
    public Guid CustomerId { get; init; }
    public Guid? DealId { get; init; }
    public Guid OwnerId { get; init; }
    public string Currency { get; init; } = null!;
    public decimal Subtotal { get; init; }
    public decimal DiscountTotal { get; init; }
    public decimal TaxTotal { get; init; }
    public decimal GrandTotal { get; init; }
    public DateTimeOffset? ValidUntil { get; init; }
    public Guid? PdfFileId { get; init; }
    public Guid? SourceDocumentId { get; init; }
    public IReadOnlyList<SalesDocumentLineDto> Lines { get; init; } = [];
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

/// <summary>
/// Базовый «лёгкий» ViewModel документа для грида (без строк). Абстрактен: наследник объявляет
/// <c>sealed record SalesDocumentGridViewModel : SalesDocumentGridViewModelBase</c>.
/// </summary>
public abstract record SalesDocumentGridViewModelBase : ICrmResponse
{
    public Guid Id { get; init; }
    public DocType DocType { get; init; }
    public string Number { get; init; } = null!;
    public DocumentStatus Status { get; init; }
    public Guid CustomerId { get; init; }
    public string Currency { get; init; } = null!;
    public decimal GrandTotal { get; init; }
    public DateTimeOffset? ValidUntil { get; init; }
}
