using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Cheetah.Modules.SalesDocuments.Shared;

namespace Cheetah.Modules.SalesDocuments.Contracts;

/// <summary>Строка документа (снимок наименования и цены из каталога на момент добавления).</summary>
public sealed record SalesDocumentLineDto(
    Guid Id, Guid ProductId, string Name, decimal UnitPrice, decimal Qty,
    decimal DiscountPercent, decimal TaxRate, decimal LineTotal);

/// <summary>
/// Тело строки. Если <see cref="UnitPriceOverride"/> не задан — цена берётся из каталога (снимок);
/// <see cref="Name"/> опционально (иначе — наименование товара из каталога).
/// </summary>
public sealed record AddLineRequest(
    Guid ProductId, decimal Qty, decimal? UnitPriceOverride, string? Name,
    decimal DiscountPercent, decimal TaxRate);

// ── Запросы операций (несут Id из маршрута; маппятся на конкретные команды через Mapster) ────────

/// <summary>Добавить строку в документ (тело + Id из маршрута).</summary>
public sealed record AddLineToDocumentRequest(
    [FromRoute] Guid Id, Guid ProductId, decimal Qty, decimal? UnitPriceOverride, string? Name,
    decimal DiscountPercent, decimal TaxRate) : ICrmRequest;

/// <summary>Удалить строку документа.</summary>
public sealed record RemoveLineRequest([FromRoute] Guid Id, [FromRoute] Guid LineId) : ICrmRequest;

/// <summary>Выпустить документ (присвоить номер).</summary>
public sealed record IssueDocumentRequest([FromRoute] Guid Id) : ICrmRequest;

/// <summary>Принять КП.</summary>
public sealed record AcceptQuoteRequest([FromRoute] Guid Id) : ICrmRequest;

/// <summary>Отклонить КП.</summary>
public sealed record RejectQuoteRequest([FromRoute] Guid Id, string? Reason) : ICrmRequest;

/// <summary>Отметить счёт оплаченным.</summary>
public sealed record MarkInvoicePaidRequest([FromRoute] Guid Id) : ICrmRequest;

/// <summary>Аннулировать документ.</summary>
public sealed record CancelDocumentRequest([FromRoute] Guid Id, string? Reason) : ICrmRequest;

/// <summary>Создать документ «на основе» (Quote→Order→Invoice).</summary>
public sealed record ConvertDocumentRequest([FromRoute] Guid Id, DocType ToType) : ICrmRequest;

/// <summary>Сгенерировать PDF документа.</summary>
public sealed record GenerateDocumentPdfRequest([FromRoute] Guid Id) : ICrmRequest;
